using DomainModels.Models.IntermediateModels;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Models.Game
{
    [MessagePackObject]
    public class BeeGamesInvitations
    {
        [Key(0)]
        public GameInviteType InviteType { get; set; }
        [Key(1)]
        public UserBaseModel RivalPlayer { get; set; }
    }

    public enum GameInviteType
    {
        YouAreChallenged = 1,
        YouCouldChallenge = 2,
        BotForFastPlay = 3
    }

}
