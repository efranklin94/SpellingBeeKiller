using MessagePack;

namespace DomainModels.Models.Game;

[MessagePackObject]
public class GameHistory : BaseDbModel
{
    [Key(1)]
    public string? WinnerName { get; set; }
    [Key(2)]
    public string? LoserName { get; set; }
    [Key(3)]
    public int? RewardedCoinAmount { get; set; }
    [Key(4)]
    public int? Score { get; set; }
    [Key(5)]
    public bool Claimed { get; set; }
    [Key(6)]
    public DateTime CreatedAt { get; set; }
}
