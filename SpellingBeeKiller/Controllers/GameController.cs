using DomainModels.Models;
using Microsoft.AspNetCore.Mvc;

namespace MainApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly GameHub gameHub;
        public GameController(GameHub gameHub)
        {
            this.gameHub = gameHub;
        }

        [HttpPost]
        public async Task<IActionResult> CreateGame()
        {
            await gameHub.SendMessage("Sample game created");

            return Ok();
        }
    }
}
