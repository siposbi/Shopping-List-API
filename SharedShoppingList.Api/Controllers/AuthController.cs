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
            return Ok(await _identityService.LoginAsync(loginModel));
        }

        [HttpPost("register")]
        public async Task<ActionResult<ResponseModel<long>>> RegisterAsync([FromBody] RegisterModel registerModel)
        {
            return Ok(await _identityService.RegisterAsync(registerModel));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ResponseModel<TokenModel>>> Refresh([FromBody] TokenModel request)
        {
            return Ok(await _identityService.RefreshTokenAsync(request));
        }
    }
}