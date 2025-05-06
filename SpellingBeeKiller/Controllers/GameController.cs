using DomainModels.DTO.ResponseModels.Game;
using DomainModels.Models;
using DomainModels.Models.IntermediateModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using static DomainModels.Models.GameHub;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json;

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
            // MOCK MATCHMAKING
            UserBaseModel host = new UserBaseModel()
            {
                UserId = "68120330ce7a8198383442e3",
                NickName = "بازیکنbXI6VMp-",
                Level = 1,
            };

            UserBaseModel guest = new UserBaseModel()
            {
                UserId = "bot0000",
                NickName = "Sam",
                Level = 4,
            };

            CoreBeeGameData gameData = new CoreBeeGameData()
            {
                GameId = "game00",
                PlayerRoomHost = host,
                PlayerRoomGuest = guest,
                RoundLogs = new List<CoreBeeGameRoundLog>(),
            };

            SocketPack socketPack = new SocketPack()
            {
                EventType = HubEvents.GameStart,
                SerializedData = JsonConvert.SerializeObject(gameData),
            };
            string msg = JsonConvert.SerializeObject(socketPack);

            await hubContext.Clients.All.SendAsync("RecievedMessage", "playerName", msg);

            
            CreateGameResponse createGameResponse = new CreateGameResponse()
            {
                GameId = "game00",
                UpdatedCoinValue = 450
            };
            return Ok(createGameResponse);
        }
    }
}
