using DomainModels.Models.IntermediateModels;
using DomainServices.Implementations;
using DomainServices.Implementations.UserServices;
using Microsoft.AspNetCore.Mvc;

namespace MainApplication.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly LoadService loadService;
        private readonly ResponseFactory responseFactory;

        public PlayerController(LoadService loadService, ResponseFactory responseFactory)
        {
            this.loadService = loadService;
            this.responseFactory = responseFactory;
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

                PlayerProfileModel response = await responseFactory.PlayerProfileModel(result.User);

                return Ok(response);
            }
            else
            {
                return BadRequest("User not found!");
            }
        }
    }
}
