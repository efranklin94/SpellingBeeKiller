using MessagePack;
using MongoDB.Bson.Serialization.Attributes;

namespace DomainModels.Models
{
    [BsonIgnoreExtraElements]
    [MessagePackObject]
    public class User : BaseDbModel
    {
        [Key(1)]
        public string Username { get; set; }
        [Key(2)]
        public int Coin { get; set; }
        [Key(3)]
        public int Level { get; set; }
        [Key(4)]
        public bool IsEmulator { get; set; }
        [Key(5)]
        public required string ActiveDeviceId { get; set; }
        [Key(6)]
        public string? Email { get; set; }
        [Key(7)]
        public long XP { get; set; }
        [Key(8)]
        public DateTime CreatedAt { get; set; }
        [Key(9)]
        public DateTime UpdatedAt { get; set; }
        [Key(10)]
        public bool UserHasChangedUsername { get; set; } = false;
        [Key(11)]
        public List<string> PreviousUsernames { get; set; } = new List<string>();
        [Key(12)]
        public string? ClientVersion { get; set; }
        [Key(13)]
        public List<string> StickerInventory { get; set; } = new List<string>();
        [Key(14)]
        public string? ReferralCode { get; set; }
        [Key(15)]
        public string RefreshToken { get; set; }
        [Key(16)]
        public bool HasGotReferralReward { get; set; }
        [Key(17)]
        public List<string> CurrentBaseGames { get; set; } = new List<string>();
        [Key(18)]
        public List<string> CurrentGames { get; set; } = new List<string>();
        [Key(19)]
        public int Ticket { get; set; }
    }
}
