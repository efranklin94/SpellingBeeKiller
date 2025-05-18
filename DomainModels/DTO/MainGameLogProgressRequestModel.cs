using DomainModels.Models.Game;

namespace DomainModels.DTO;

public class MainGameLogProgressRequestModel
{
    public required string GameId { get; set; }
    public required CoreBeeGameRoundLog Round {  get; set; }
}
