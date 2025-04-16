using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Models.IntermediateModels
{
    public class PlayerProgressModel
    {
        public int Coin { get; set; }
        public int Level { get; set; }

        public List<ClassicModeData> ClassicModeDataList { get; set; }
    }
}
