using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTO.RequestModels.Auth;

public class CheckExistsRequestModel
{
    [Required]
    public required string DeviceId { get; set; }
}
