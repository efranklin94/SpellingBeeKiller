using DomainModels.Models.IntermediateModels;
using MessagePack;

namespace DomainModels.Models.Game
{
    [MessagePackObject]
    public class CoreBeeGameData : BaseDbModel
    {
        [Key(1)]
        public string GameId { get; set; }
        [Key(2)]
        public string PlayerRoomHostId { get; set; }
        [Key(3)]
        public string PlayerRoomGuestId { get; set; }
        [Key(4)]
        public int TimePerTurnInHours { get; set; } = 12;
        [Key(5)]
        public List<CoreBeeGameRoundLog> RoundLogs { get; set; } = new List<CoreBeeGameRoundLog>();
        [Key(6)]
        public DateTime CreatedAt { get; set; }
        [Key(7)]
        public DateTime UpdatedAt { get; set; }
    }
}
