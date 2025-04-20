using DomainModels.Models.IntermediateModels;
using MessagePack;

namespace DomainModels.Models
{
    [MessagePackObject]
    public class CoreBeeGameData : BaseDbModel
    {
        [Key(1)]
        public string GameId { get; set; }
        [Key(2)]
        public string FirstPlayer {  get; set; }
        [Key(3)]
        public string SecondPlayer { get; set; }
        [Key(4)]
        public UserBaseModel PlayerRoomHost { get; set; }
        [Key(5)]
        public UserBaseModel PlayerRoomGuest { get; set; }
        [Key(6)]
        public int TimePerTurnInHours { get; set; } = 12;
        [Key(7)]
        public List<CoreBeeGameRoundLog> RoundLogs { get; set; } = new List<CoreBeeGameRoundLog>();
        [Key(8)]
        public DateTime CreatedAt { get; set; }
        [Key(9)]
        public DateTime UpdatedAt { get; set; }
    }
}
