namespace DomainModels.DTO.ResponseModels.Auth
{
    [MessagePack.MessagePackObject]
    public class RecoveryResponse
    {
        [MessagePack.Key(0)]
        public string UserId { get; set; }
        [MessagePack.Key(1)]
        public string Username { get; set; }
        [MessagePack.Key(2)]
        public string RefreshToken { get; set; }
    }
}
