using MessagePack;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace DomainModels.Models
{
    [BsonIgnoreExtraElements]
    [MessagePackObject]
    public class User : BaseDbModel
    {
        [Key(1)]
        public string UserId { get; set; }
        [Key(2)]
        public string Username { get; set; }
        [Key(3)]
        public int Coin { get; set; }
        [Key(4)]
        public int Level { get; set; }
        [Key(5)]
        public bool IsEmulator { get; set; }
        [Key(6)]
        public required string ActiveDeviceId { get; set; }
        [Key(7)]
        public string? Email { get; set; }
        [Key(8)]
        public long XP { get; set; }
        [Key(9)]
        public DateTime CreatedAt { get; set; }
        [Key(10)]
        public DateTime UpdatedAt { get; set; }
        [Key(11)]
        public bool UserHasChangedUsername { get; set; } = false;
        [Key(12)]
        public List<string> PreviousUsernames { get; set; } = new List<string>();
        [Key(13)]
        public string? ClientVersion { get; set; }
        [Key(14)]
        public List<string> StickerInventory { get; set; } = new List<string>();
        [Key(15)]
        public string? ReferralCode { get; set; }
        [Key(16)]
        public string RefreshToken { get; set; }
        [Key(17)]
        public bool HasGotReferralReward { get; set; }
        [Key(18)]
        public List<string> CurrentBaseGames { get; set; } = new List<string>();
        [Key(19)]
        public List<string> CurrentGames { get; set; } = new List<string>();
    }
}
