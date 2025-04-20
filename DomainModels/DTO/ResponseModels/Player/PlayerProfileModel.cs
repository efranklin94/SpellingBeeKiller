using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.DTO.ResponseModels.Player
{
    public class PlayerProfileModel : PlayerProgressModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
    }
}
