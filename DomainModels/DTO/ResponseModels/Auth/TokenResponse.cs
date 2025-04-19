namespace DomainModels.DTO.ResponseModels.Auth
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public string GrantType { get; set; } = "password";
    }
}
