using DomainModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MainApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<GameHub> hubContext;

        public GameController(IHubContext<GameHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateGame()
        {
            await hubContext.Clients.All.SendAsync("RecievedMessage", "playerName", "Sample game created");
            return Ok();
        }
    }
}
