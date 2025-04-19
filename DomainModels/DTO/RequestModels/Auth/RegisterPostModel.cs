using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTO.RequestModels.Auth
{
    public class RegisterPostModel
    {
        [Required]
        public string DeviceId { get; set; }
        //public bool IsEmulator { get; set; }
    }
}
