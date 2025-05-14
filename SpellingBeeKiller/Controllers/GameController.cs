using DomainModels.DTO;
using DomainModels.DTO.ResponseModels.Game;
using DomainModels.Models;
using DomainServices.Implementations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Repositories.Contracts;

namespace MainApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IHubContext<GameHub> hubContext;
        private readonly GameService gameService;
        private readonly ILogger<GameController> logger;
        private readonly IUserRepository userRepository;

        public GameController(IHubContext<GameHub> hubContext, GameService gameService, IUserRepository userRepository, ILogger<GameController> logger)
        {
            this.hubContext = hubContext;
            this.gameService = gameService;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateGame(string userId)
        {
            var result = await gameService.CreateGameAsync(userId);
            
            CreateOrJoinGameResponse createGameResponse = new CreateOrJoinGameResponse()
            {
                GameData = result.gameDataDTO,
                UpdatedTicketValue = result.firstUserUpdatedTicket,
            };

            return Ok(createGameResponse);
        }

        [HttpPost("SaveProgress")]
        public async Task<IActionResult> SaveGameProgress(MainGameLogProgressRequestModel model)
        {
            await gameService.SaveGameProgress(model);
            
            return Ok();
        }

        [HttpPost("Finish")]
        // when were done with the whos the winner then update this code
        public async Task<IActionResult> FinishGame(string winnerUserId, string gameId)
        {
            await gameService.FinishGameAsync(winnerUserId, gameId);

            return Ok();
        }

        [HttpPost("Join")]
        public async Task<IActionResult> JoinGame(string gameId, string userId)
        {
            var result = await gameService.JoinGameAsync(gameId, userId);
            
            CreateOrJoinGameResponse joinGameResponse = new CreateOrJoinGameResponse()
            {
                GameData = result.gameData,
                UpdatedTicketValue = result.updatedTicket,
            };

            return Ok(joinGameResponse);
        }
    }
}
