using DomainModels.Models.Game;
using DomainModels.Models.IntermediateModels;

namespace DomainModels.DTO;

public class CoreBeeGameData
{
    public string GameId { get; set; }
    public UserBaseModel PlayerRoomHost { get; set; }
    public UserBaseModel PlayerRoomGuest { get; set; }
    public int TimePerTurnInHours { get; set; } = 12;
    public List<CoreBeeGameRoundLog> RoundLogs { get; set; } = new List<CoreBeeGameRoundLog>();
}
