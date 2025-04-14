using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModels.Models.IntermediateModels
{
    [BsonDiscriminator("UserBaseModel")]
    [MessagePackObject]
    public class UserBaseModel
    {
        [Key(0)]
        public string UserId { get; set; }
        [Key(1)]
        public string NickName { get; set; }
        [Key(2)]
        public string ClanName { get; set; }
        [Key(3)]
        public string ClanId { get; set; }
        [Key(7)]
        public string ClanBadgeId { get; set; }
        [Key(4)]
        public int Level { get; set; }
        [Key(5)]
        public List<string> ActiveCosmeticItems { get; set; }
        [Key(6)]
        public OnlineStatusEnum OnlineStatus { get; set; }


        // required for job scheduler
        public UserBaseModel()
        {

        }

        // TODO
        //public UserBaseModel(User user, IUserStatusService userConnectionStatusService)
        //{
        //    this.UserId = user.Id!;
        //    this.NickName = user.Username;
        //    this.ClanName = user.Clan?.ClanName!;
        //    this.ClanId = user.Clan?.ClanId!;
        //    this.ClanBadgeId = user.Clan?.ClanBadgeId!;
        //    this.Level = user.Level;
        //    this.ActiveCosmeticItems = user.ActiveCosmeticItems;
        //    if (userConnectionStatusService != null)
        //    {
        //        this.OnlineStatus = userConnectionStatusService.GetUserOnlineStatusWithPrivacySettingsInMind(user.Id!);
        //    }
        //    else
        //    {
        //        OnlineStatus = OnlineStatusEnum.Offline;
        //    }

        //}

        public UserBaseModel(User user)
        {
            this.UserId = user.Id!;
            this.NickName = user.Username;
            //this.ClanName = user.Clan?.ClanName!;
            //this.ClanId = user.Clan?.ClanId!;
            //this.ClanBadgeId = user.Clan?.ClanBadgeId!;
            this.Level = user.Level;
            //this.ActiveCosmeticItems = user.ActiveCosmeticItems;
            this.OnlineStatus = OnlineStatusEnum.Offline;
        }
    }


    public enum OnlineStatusEnum
    {
        Online = 1,
        Offline = 2,
        Busy = 3
    }
}
