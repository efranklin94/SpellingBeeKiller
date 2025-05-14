namespace DomainModels.DTO.ResponseModels.Game;

public class CreateOrJoinGameResponse
{
    public CoreBeeGameData GameData {  get; set; }
    public int UpdatedTicketValue { get; set; }
}
