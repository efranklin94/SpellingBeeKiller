using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.DTO.ResponseModels.Game
{
    public class CreateGameResponse
    {
        required public string GameId;
        public int UpdatedCoinValue;
    }
}
