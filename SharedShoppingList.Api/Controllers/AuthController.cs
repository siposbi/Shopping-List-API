using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharedShoppingList.Data.Models;
using SharedShoppingList.Data.Services;

namespace SharedShoppingList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResponseModel<TokenModel>>> LoginAsync([FromBody] LoginModel loginModel)
        {
            var result = await _identityService.LoginAsync(loginModel);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<ResponseModel<long>>> RegisterAsync([FromBody] RegisterModel registerModel)
        {
            var result = await _identityService.RegisterAsync(registerModel);
            if (result.IsSuccess)
            {
                return Created(result.Data.ToString(), result);
            }

            return BadRequest(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ResponseModel<TokenModel>>> Refresh([FromBody] TokenModel request)
        {
            var result = await _identityService.RefreshTokenAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}