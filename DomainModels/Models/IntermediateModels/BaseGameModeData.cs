using MessagePack;

namespace DomainModels.Models.IntermediateModels
{
    [MessagePackObject]
    public class BaseGameModeData
    {
        [Key(0)]
        public string GameId { get; set; }
        [Key(1)]
        public DateTime StartTime { get; set; }
        [Key(2)]
        public DateTime EndTime { get; set; }
    }
}
