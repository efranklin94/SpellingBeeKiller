using MessagePack;

namespace DomainModels.Models.Game;

[MessagePackObject]
public class CoreBeeGameRoundLog
{
    [Key(0)]
    public string Username { get; set; }
    [Key(1)]
    public DateTime TurnPlayedTime { get; set; }
}
