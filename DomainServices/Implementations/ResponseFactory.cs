using DomainModels.DTO.ResponseModels.Player;
using DomainModels.Models;
using DomainModels.Models.Game;
using DomainModels.Models.IntermediateModels;

namespace DomainServices.Implementations
{
    public class ResponseFactory
    {
        public async Task<LoadResponse> PlayerProfileModel(User user)
        {
            var model = new LoadResponse
            {
                PlayerModel = new PlayerProfileModel
                {
                    UserId = user.Id!,
                    Username = user.Username,
                    Level = user.Level,
                    Coin = user.Coin
                },
                ClassisModeDataList = new List<ClassicModeData>
            {
                new ClassicModeData
                {
                    GameId = "CLASSIC_001",
                    StartTime = DateTime.Now.AddDays(-2),
                    EndTime = DateTime.Now.AddDays(2),
                    WordToGuess = new[] { 'C', 'L', 'A', 'S', 'S', 'I', 'C' },
                    StateLogs = new List<char[]>
                    {
                        new[] { 'C', 'R', 'A', 'N', 'E', 'S', 'T' },
                        new[] { 'C', 'L', 'O', 'U', 'D', 'Y', ' ' }
                    }
                }
            },
                CoreBeeGameDataList = new List<CoreBeeGameData>
            {
                new CoreBeeGameData
                {
                    GameId = "BEEBATTLE_202",
                    FirstPlayer = "BeeMaster123",
                    SecondPlayer = "HoneyExpert",
                    PlayerRoomHost = new UserBaseModel
                    {
                        UserId = "host123",
                        NickName = "QueenBee",
                        Level = 42
                    },
                    PlayerRoomGuest = new UserBaseModel
                    {
                        UserId = "guest456",
                        NickName = "WorkerBee",
                        Level = 38
                    },
                    TimePerTurnInHours = 24,
                    RoundLogs = new List<CoreBeeGameRoundLog>
                    {
                        new CoreBeeGameRoundLog
                        {
                            Username = "WorkerBee",
                            TurnPlayedTime = DateTime.Now.AddDays(-1),
                            TurnStartTime = DateTime.Now.AddDays(-2),
                        }
                    },
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow.AddHours(-12)
                }
            },
                BeeGamesInvitations = new List<BeeGamesInvitations>
            {
                new BeeGamesInvitations
                {
                    InviteType = GameInviteType.YouAreChallenged,
                    RivalPlayer = new UserBaseModel
                    {
                        UserId = "rival001",
                        NickName = "StingerPro",
                        Level = 45
                    }
                },
                new BeeGamesInvitations
                {
                    InviteType = GameInviteType.BotForFastPlay,
                    RivalPlayer = new UserBaseModel
                    {
                        UserId = "bot_007",
                        NickName = "AI_Bee",
                        Level = 99
                    }
                }
            }
            };

            return model;
        }
    }
}
