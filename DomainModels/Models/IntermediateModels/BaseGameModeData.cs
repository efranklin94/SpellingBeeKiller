using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Models.IntermediateModels
{
    public class BaseGameModeData
    {
        public string GameId { get; set; }
        //I think its better to be string or Datetime
        public long StartTime { get; set; }
        public long EndTime { get; set; }
    }
}
