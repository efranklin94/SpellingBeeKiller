using DomainModels.Models.Game;

namespace DomainModels.DTO;

public class MainGameLogProgressRequestModel
{
    public string GameId { get; set; }
    public CoreBeeGameRoundLog Round {  get; set; }
}
