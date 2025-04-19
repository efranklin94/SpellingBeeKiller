using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTO.RequestModels.Auth
{
    public class GetTokenRequestModel
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
