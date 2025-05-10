using DomainModels.DTO.RequestModels.Auth;
using DomainModels.DTO.ResponseModels.Auth;
using DomainServices.Contracts;
using DomainServices.Contracts.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MainApplication.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IAuthService authService;

        public AuthController(IUserService service, IAuthService authService, IConfiguration configuration)
        {
            this.userService = service;
            this.authService = authService;
        }

        //[AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            var result = await userService.CreateUserAsync(model.DeviceId);

            if (result.error != null)
            {
                return BadRequest(result.error);
            }

            //result.user.RefreshToken = await authService.RenewRefreshToken(result.user, model.DeviceId);
            //var accessTokenResult = await authService.GetAccessToken(result.user.RefreshToken);

            var response = new RegisterResponse
            {
                Id = result.user.Id,
                RefreshToken = result.user.RefreshToken,
                Username = result.user.Username,
                //AccessToken = accessTokenResult.error == null ? accessTokenResult.token : null
            };

            return Ok(response);
        }


        //[AllowAnonymous]
        //[HttpPost("token")]
        //public async Task<ActionResult<string>> Token(GetTokenRequestModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Invalid Request");
        //    }

        //    var result = await authService.GetAccessToken(model.RefreshToken);

        //    if (result.error != null)
        //    {
        //        return BadRequest(result.error);
        //    }

        //    var tokenResponse = new TokenResponse()
        //    {
        //        Token = result.token
        //    };

        //    return Ok(tokenResponse);
        //}

    }
}