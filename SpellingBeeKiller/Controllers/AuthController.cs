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
        //private readonly ResponseCachingService responseCachingService;
        //private readonly string defaultLocale;

        public AuthController(IUserService service, IAuthService authService,
            //ResponseCachingService responseCachingService,
            IConfiguration configuration)
        {
            this.userService = service;
            this.authService = authService;
            //this.responseCachingService = responseCachingService;
            //defaultLocale = configuration["ProjectSettings:defaultLocale"];
        }




        //[AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            //var locale = defaultLocale;
            //if (HttpContext.Request.Headers.TryGetValue("CF-IPCountry", out var countryCode))
            //{
            //    locale = countryCode;
            //}


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

        //    //var result = await authService.GetAccessToken(model.RefreshToken);

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

        //[AllowAnonymous]
        //[HttpPost("getRecoveryCodeByEmail")]
        //public async Task<ActionResult<string>> GetRecoveryCodeByEmail(GetRecoveryCodeByEmail model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Invalid Request");
        //    }

        //    var result = await authService.GetRecoveryCodeByEmail(model.Email);

        //    if (result.error != null)
        //    {
        //        return BadRequest(result.error);
        //    }

        //    return Ok("{}");
        //}

        //[HttpGet("SetEmail")]
        //public async Task<ActionResult<string>> SetEmail(string newEmail)
        //{
        //    if (string.IsNullOrEmpty(newEmail))
        //    {
        //        return BadRequest("Check input!");
        //    }

        //    if (!IsValidEmail(newEmail))
        //    {
        //        return BadRequest("Email is not valid");
        //    }


        //    var userId = HttpContext.User.Claims
        //        .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

        //    var result = await authService.SetEmail(newEmail);

        //    if (result.error != null)
        //    {
        //        return BadRequest(result.error);
        //    }

        //    return Ok("{}");

        //    bool IsValidEmail(string email)
        //    {
        //        try
        //        {
        //            var addr = new System.Net.Mail.MailAddress(email);
        //            return addr.Address == email;
        //        }
        //        catch
        //        {
        //            return false;
        //        }
        //    }
        //}

        //[HttpPost("VerifyEmail")]
        //public async Task<ActionResult<string>> VerifyEmail(string code, string email)
        //{
        //    var userId = HttpContext.User.Claims
        //        .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

        //    (bool success, string error) = await authService.VerifyEmail(userId, code, email);

        //    if (error != null)
        //    {
        //        return BadRequest(error);
        //    }

        //    return Ok("{}");
        //}



        //[AllowAnonymous]
        //[HttpPost("recoverByRecoveryCode")]
        //public async Task<ActionResult<string>> RecoverByRecoveryCode(RecoverByRecoveryCodePostModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Invalid Request");
        //    }

        //    var tag = HttpContext.Request.Headers.First(x => x.Key == "tag").Value.ToString();

        //    // check for cached response
        //    var cachedResponse = await responseCachingService.ReadFromCache<RecoveryResponse>(tag);

        //    if (cachedResponse != null)
        //    {
        //        return Ok(cachedResponse);
        //    }

        //    (User user, string error) =
        //        await authService.RecoverAccountByRecoveryCode(model.RecoveryCode, model.NewDeviceId);

        //    if (error != null)
        //    {
        //        return BadRequest(error);
        //    }


        //    if (string.IsNullOrEmpty(user.RefreshToken))
        //    {
        //        throw new System.Exception("Failed to generate new refresh token");
        //    }

        //    var response = new RecoveryResponse
        //    {
        //        RefreshToken = user.RefreshToken,
        //        UserId = user.Id,
        //        Username = user.Username
        //    };

        //    // save to cache
        //    await responseCachingService.SaveToCache(tag, response);

        //    return Ok(response);
        //}

        //[AllowAnonymous]
        //[HttpPost("recoverByDeviceId")]
        //public async Task<ActionResult<string>> RecoverByDeviceId(RecoverByDeviceIdPostModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Invalid Request");
        //    }

        //    var result = await authService.RecoverAccountByDeviceId(model.DeviceId, model.Proof);

        //    if (result.error != null)
        //    {
        //        return BadRequest(result.error);
        //    }

        //    return Ok(new RecoveryResponse
        //    {
        //        RefreshToken = result.user.RefreshToken,
        //        UserId = result.user.Id,
        //        Username = result.user.Username
        //    });
        //}

        //[HttpPost("externalLogin")]
        //public async Task<ActionResult<string>> LoginWithFacebook(ExternalAuthLoginModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Invalid Request");
        //    }

        //    var userId = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        //    if (userId == null)
        //    {
        //        return BadRequest("user not found!");
        //    }

        //    var result = await authService.ExternalAuth(userId, model.ExternalAuthToken,
        //        model.ActiveDeviceId, model.ExternalAuthProvider, model.UniqueId);

        //    if (result.error != null)
        //    {
        //        return BadRequest(result.error);
        //    }

        //    return Ok(new RecoveryResponse
        //    {
        //        RefreshToken = result.oldUser?.RefreshToken,
        //        UserId = result.oldUser?.Id,
        //        Username = result.oldUser?.Username
        //    });
        //}

        //[HttpPost("SetPhoneNumber")]
        //public async Task<ActionResult<string>> SetPhoneNumber(string phoneNumber)
        //{
        //    if (string.IsNullOrEmpty(phoneNumber))
        //    {
        //        return BadRequest("Check input!");
        //    }

        //    if (!(phoneNumber.Length == 11 && phoneNumber.StartsWith("09")))
        //    {
        //        return BadRequest("PhoneNumber is not valid");
        //    }

        //    var userId = HttpContext.User.Claims
        //        .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

        //    var result = await authService.SetPhoneNumber(userId, phoneNumber);
        //    if (result.error != null)
        //    {
        //        return BadRequest(result.error);
        //    }

        //    return Ok("{}");
        //}
        
        //[HttpPost("VerifyPhoneNumber")]
        //public async Task<ActionResult<string>> VerifyPhoneNumber(string code, string phoneNumber)
        //{
        //    var userId = HttpContext.User.Claims
        //        .SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

        //    (bool success, string error) = await authService.VerifyPhoneNumber(userId, code, phoneNumber);

        //    if (error != null)
        //    {
        //        return BadRequest(error);
        //    }

        //    return Ok("{}");
        //}
    }
}