using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Models.IntermediateModels
{
    [MessagePackObject]
    public class CoreBeeGameRoundLog
    {
        [Key(0)]
        public string Username { get; set; }
        [Key(1)]
        public DateTime TurnStartTime { get; set; }
        [Key(2)]
        public DateTime TurnPlayedTime { get; set; }
    }

}
