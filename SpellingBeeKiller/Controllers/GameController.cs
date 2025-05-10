using DomainModels.DTO.ResponseModels.Game;
using DomainModels.Models;
using DomainModels.Models.Game;
using DomainModels.Models.IntermediateModels;
using DomainServices.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MainApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<GameHub> hubContext;
        private readonly GameService gameService;
        private readonly ILogger<GameController> logger;

        public GameController(IHubContext<GameHub> hubContext, GameService gameService)
        {
            this.hubContext = hubContext;
            this.gameService = gameService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateGame(string firstUserId, string secondUserId)
        {
            CoreBeeGameData coreBeeGameData = await gameService.CreateGameAsync(firstUserId, secondUserId);

            await hubContext.Clients.User(secondUserId).SendAsync("CreateGame", coreBeeGameData);
            
            CreateGameResponse createGameResponse = new CreateGameResponse()
            {
                GameId = coreBeeGameData.GameId,
                UpdatedCoinValue = 450,
            };

            return Ok(createGameResponse);
        }

        [HttpPost("Finish")]
        public async Task<IActionResult> FinishGame(MainGameHistory gameHistory)
        {
            await gameService.FinishGameAsync(gameHistory);

            return Ok();
        }
    }
}
