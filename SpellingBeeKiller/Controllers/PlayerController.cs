using DomainModels.DTO.RequestModels.Auth;
using DomainModels.DTO.ResponseModels.Player;
using DomainModels.Models;
using DomainServices.Contracts;
using DomainServices.Implementations;
using DomainServices.Implementations.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace MainApplication.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly LoadService loadService;
        private readonly ResponseFactory responseFactory;

        public PlayerController(LoadService loadService, ResponseFactory responseFactory, IAuthService authService)
        {
            this.loadService = loadService;
            this.responseFactory = responseFactory;
            this.authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("checkUserExists")]
        public async Task<ActionResult<string>> CheckExists(CheckExistsRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Request");
            }

            (bool userExists, User user) = await authService.UserExistsWithDeviceIdAsync(model.DeviceId);

            if (userExists)
            {
                return Ok(new JObject { { "Username", user.Username } });
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet("load")]
        //TODO client version, storetype, validgpzone
        public async Task<ActionResult<string>> Load(string userId)
        {
            //var userId = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            //var deviceIdClaim = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.SerialNumber);
            //var refreshTokenHash = HttpContext.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Hash).Value;

            if (userId != null)
            {
                var result = await loadService.Load(userId);

                if (result.error != null)
                {
                    return BadRequest(result.error);
                }

                LoadResponse response = await responseFactory.PlayerProfileModel(result.User);

                return Ok(response);
            }
            else
            {
                return BadRequest("User not found!");
            }
        }
    }
}
