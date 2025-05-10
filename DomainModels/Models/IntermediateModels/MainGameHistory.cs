namespace DomainModels.Models.IntermediateModels;

public class MainGameHistory
{
    public required string GameId { get; set; }
    public string? WinnerName { get; set; }
    public string? LoserName { get; set; }
    public int RewardedCoinAmount { get; set; }
    public int Score {  get; set; }
    public bool Claimed { get; set; }
}
