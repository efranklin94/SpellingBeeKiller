using DomainModels.Models;
using DomainModels.Models.IntermediateModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainServices.Implementations
{
    public class ResponseFactory
    {
        public async Task<PlayerProfileModel> PlayerProfileModel(User user)
        {
            PlayerProfileModel model = new PlayerProfileModel();

            model.Username = user.Username;
            model.Level = user.Level;
            //model.ClassicModeDataList = 
            model.Coin = user.Coin;
        
            return model;
        }
    }
}
