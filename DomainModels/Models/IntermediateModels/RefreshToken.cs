namespace DomainModels.Models.IntermediateModels;

public class RefreshToken
{
    public required string DeviceId { get; set; }
    public DateTime ExpireAt { get; set; }
    public int RandomNounce { get; set; }
    public required string UserId { get; set; }
}