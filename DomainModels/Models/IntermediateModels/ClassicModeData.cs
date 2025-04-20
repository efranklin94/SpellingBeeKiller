using MessagePack;

namespace DomainModels.Models.IntermediateModels
{
    [MessagePackObject]
    public class ClassicModeData : BaseGameModeData
    {
        [Key(5)]
        public char[] WordToGuess { get; set; }
        [Key(6)]
        public List<char[]> StateLogs { get; set; } = new List<char[]>();
    }
}
