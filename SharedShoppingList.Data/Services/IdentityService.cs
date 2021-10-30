using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedShoppingList.data;
using SharedShoppingList.Data.Entities;
using SharedShoppingList.Data.Models;

namespace SharedShoppingList.Data.Services
{
    public interface IIdentityService
    {
        Task<ResponseModel<TokenModel>> LoginAsync(LoginModel login);
        Task<ResponseModel<long>> RegisterAsync(RegisterModel registerModel);
        Task<ResponseModel<TokenModel>> RefreshTokenAsync(TokenModel request);
    }

    public class IdentityService : IIdentityService
    {
        private readonly ShoppingListDbContext _context;
        private readonly ServiceConfiguration _appSettings;

        private readonly TokenValidationParameters _tokenValidationParameters;

        public IdentityService(ShoppingListDbContext context,
            IOptions<ServiceConfiguration> settings,
            TokenValidationParameters tokenValidationParameters)
        {
            _context = context;
            _appSettings = settings.Value;
            _tokenValidationParameters = tokenValidationParameters;
        }


        public async Task<ResponseModel<TokenModel>> LoginAsync(LoginModel login)
        {
            var response = new ResponseModel<TokenModel>();
            try
            {
                var loginUser =
                    await _context.Users.SingleOrDefaultAsync(
                        c => c.Email == login.Email && c.Password == login.Password);

                if (loginUser == null)
                {
                    return response.Unsuccessful("Invalid Username or Password.");
                }
                
                var authenticationResult = await AuthenticateAsync(loginUser);
                if (authenticationResult is { Success: true })
                {
                    response.Data = new TokenModel
                        { Token = authenticationResult.Token, RefreshToken = authenticationResult.RefreshToken };
                }
                else
                {
                    return response.Unsuccessful("Something went wrong!");
                }

                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<long>> RegisterAsync(RegisterModel registerModel)
        {
            var response = new ResponseModel<long>();
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == registerModel.Email))
                {
                    return response.Unsuccessful("Email already registered!");
                }

                var user = new User
                {
                    Password = registerModel.Password,
                    Email = registerModel.Email,
                    FirstName = registerModel.FirstName,
                    LastName = registerModel.LastName
                };
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                response.Data = user.Id;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        public async Task<ResponseModel<TokenModel>> RefreshTokenAsync(TokenModel request)
        {
            var response = new ResponseModel<TokenModel>();
            try
            {
                var authResponse = await GetRefreshTokenAsync(request.Token, request.RefreshToken);
                if (!authResponse.Success)
                {
                    return response.Unsuccessful(string.Join(",", authResponse.Errors));
                }

                var refreshTokenModel = new TokenModel
                {
                    Token = authResponse.Token,
                    RefreshToken = authResponse.RefreshToken
                };
                response.Data = refreshTokenModel;
                return response;
            }
            catch (Exception)
            {
                return response.Exception();
            }
        }

        private async Task<AuthenticationResult> AuthenticateAsync(User user)
        {
            var authenticationResult = new AuthenticationResult();
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var key = Encoding.ASCII.GetBytes(_appSettings.JwtSettings.Secret);

                var subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                });

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = subject,
                    Expires = DateTime.UtcNow.AddDays(_appSettings.JwtSettings.TokenLifetimeInDays),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                authenticationResult.Token = tokenHandler.WriteToken(token);


                var refreshToken = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString(),
                    JwtId = token.Id,
                    UserId = user.Id,
                    CreationDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddMonths(1)
                };
                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();
                authenticationResult.RefreshToken = refreshToken.Token;
                authenticationResult.Success = true;
                return authenticationResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<AuthenticationResult> GetRefreshTokenAsync(string token, string refreshToken)
        {
            var validatedToken = GetPrincipalFromToken(token);

            if (validatedToken == null)
            {
                return new AuthenticationResult { Errors = new[] { "Invalid Token" } };
            }

            var expiryDateUnix =
                long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            if (expiryDateTimeUtc > DateTime.UtcNow)
            {
                return new AuthenticationResult { Errors = new[] { "This token hasn't expired yet" } };
            }

            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = _context.RefreshTokens.FirstOrDefault(x => x.Token == refreshToken);

            if (storedRefreshToken == null)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token does not exist" } };
            }

            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token has expired" } };
            }

            if (storedRefreshToken.Used is true)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token has been used" } };
            }

            if (storedRefreshToken.JwtId != jti)
            {
                return new AuthenticationResult { Errors = new[] { "This refresh token does not match this JWT" } };
            }

            storedRefreshToken.Used = true;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();
            var strUserId = validatedToken.Claims.Single(x => x.Type == "UserId").Value;
            long.TryParse(strUserId, out var userId);
            var user = await _context.Users.SingleOrDefaultAsync(c => c.Id == userId);
            if (user == null)
            {
                return new AuthenticationResult { Errors = new[] { "User Not Found" } };
            }

            return await AuthenticateAsync(user);
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenValidationParameters = _tokenValidationParameters.Clone();
                tokenValidationParameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                return !IsJwtWithValidSecurityAlgorithm(validatedToken) ? null : principal;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return validatedToken is JwtSecurityToken jwtSecurityToken &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase);
        }
    }
}