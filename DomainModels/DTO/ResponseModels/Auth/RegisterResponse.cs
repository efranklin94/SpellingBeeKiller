namespace DomainModels.DTO.ResponseModels.Auth
{
    public class RegisterResponse
    {
        public string Id { get; set; }

        public string Username { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}
