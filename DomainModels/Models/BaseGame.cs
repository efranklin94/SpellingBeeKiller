using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Models
{
    [MessagePackObject]
    public class BaseGame : BaseDbModel
    {
        [Key(1)]
        public string BaseGameId { get; set; }
        [Key(2)]
        public string FirstPlayer {  get; set; }
        [Key(3)]
        public string SecondPlayer { get; set; }
        [Key(4)]
        public string CurrentTurn { get; set; }
        [Key(5)]
        public DateTime Deadline { get; set; }
        //[Key(6)]
        //public BaseGameStats BaseGameStats { get; set; }
        [Key(7)]
        public PlayModeStatus PlayModeStatus { get; set; }
    }

    public enum PlayModeStatus
    {
        Start = 1,
        Play = 2, 
        Stop = 3
    }
}
