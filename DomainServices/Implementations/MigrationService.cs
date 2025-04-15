using DomainModels.Models;
using DomainModels.Models.IntermediateModels;
using MongoDB.Driver;
using Repositories.Contracts;
using System.Formats.Asn1;

namespace DomainServices.Implementations
{
    public class MigrationService
    {
        private readonly IUserRepository userRepository;
        private readonly Random random;

        public MigrationService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;

            this.random = new Random();
        }

        //public static void AddInitialCosmeticItems(User user)
        //{
        //    user.CosmeticInventory = new List<string>
        //    {
        //        "hair0","hair1","hair2","hair3",
        //        "skin0","skin1","skin2","skin3","skin4",
        //        "eye0","eye1","eye2","eye3",
        //        "mouth0","mouth1","mouth2","mouth3",
        //        "acc"
        //    };

        //    AddInitialActiveCosmeticItems(user);
        //}

        //public static void AddInitialActiveCosmeticItems(User user)
        //{
        //    user.ActiveCosmeticItems = new List<string>
        //    {
        //        "hair0","skin0","eye0", "mouth0", "acc"
        //    };

        //    // TODO: check release modes
        //    // var releaseModeGlobal = Environment.GetEnvironmentVariable("RELEASE_MODE_GLOBAL");
        //    // bool isGlobalRelease = !string.IsNullOrWhiteSpace(releaseModeGlobal);
        //    // if (isGlobalRelease)
        //    // {
        //    //     user.ActiveCosmeticItems.Add("daubdefault");
        //    // }
        //    // else
        //    // {
        //    //     user.ActiveCosmeticItems.Add("daub0");
        //    // }

        //    user.ActiveCosmeticItems.Add("daubdefault");
        //}

        //public static void AddInitialStickers(User user)
        //{
        //    user.StickerInventory = new List<string>
        //    {
        //        "stickerEmoji1",
        //        "stickerEmoji2",
        //        "stickerEmoji3",
        //        "stickerEmoji4",
        //        "stickerEmoji5",
        //        "stickerEmoji6",
        //        "stickerEmoji7",
        //        "stickerEmoji8",
        //        "stickerEmoji9",
        //        "stickerEmoji10",
        //    };
        //}

        //public async Task AddReferralCodeAsync(User user)
        //{
        //    // loop to create a unique referal code
        //    int tries = 10;
        //    while (true)
        //    {
        //        var newCode = $"{Nanoid.Nanoid.Generate("ABCDEFGHIJKLMNOPQRSTUVWXYZ", random.Next(2, 4))}-{Nanoid.Nanoid.Generate("0123456789", random.Next(3, 5))}-{Nanoid.Nanoid.Generate("ABCDEFGHIJKLMNOPQRSTUVWXYZ", random.Next(3, 5))}";
        //        var oldUser = await userRepository.GetUserByReferralCodeAsync(newCode);
        //        if (oldUser == null)
        //        {
        //            user.ReferralCode = newCode;
        //            break;
        //        }
        //        tries--;
        //        if (tries < 0)
        //        {
        //            throw new Exception("Failed to find a unique referal code for user");
        //        }
        //    }
        //}

        public async Task<(bool needUpdate, UpdateDefinition<User> updateDefinition)> MigrateAndUpdateUserData(User user, string storeType, string clientVersion)
        {
            // check release modes
            var releaseModeGlobal = Environment.GetEnvironmentVariable("RELEASE_MODE_GLOBAL");
            bool isGlobalRelease = !string.IsNullOrWhiteSpace(releaseModeGlobal);

            var update = Builders<User>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow);
            bool needUpdate = false;
            // powerups
            //if (user.Powerups == null)
            //{
            //    AddInitialPowerups(user);
            //    update = update.Set(x => x.Powerups, user.Powerups);
            //    needUpdate = true;
            //}
            //else if (user.Powerups.Count == 0)
            //{
            //    AddInitialPowerups(user);
            //    update = update.Set(x => x.Powerups, user.Powerups);
            //    needUpdate = true;
            //}

            // friends
            //if (user.Friends == null)
            //{
            //    AddInitialFriendsItems(user);
            //    update = update.Set(x => x.Friends, user.Friends);
            //    update = update.Set(x => x.FriendRequestsReceived, user.FriendRequestsReceived);
            //    needUpdate = true;
            //}

            //tournaments History
            //if (user.TournamentsHistory == null)
            //{
            //    user.TournamentsHistory = new List<UserTournaments>(TournamentHistoryQueueSize);
            //    update = update.Set(x => x.TournamentsHistory, user.TournamentsHistory);
            //    needUpdate = true;
            //}

            // initial cosmetic items
            //if (user.CosmeticInventory == null)
            //{
            //    AddInitialCosmeticItems(user);
            //    update = update.Set(x => x.CosmeticInventory, user.CosmeticInventory);
            //    update = update.Set(x => x.ActiveCosmeticItems, user.ActiveCosmeticItems);
            //    needUpdate = true;
            //}

            //if (user.StickerInventory == null || user.StickerInventory?.Count == 0)
            //{
            //    AddInitialStickers(user);
            //    update = update.Set(x => x.StickerInventory, user.StickerInventory);
            //    needUpdate = true;
            //}

            //if (user.ActiveCosmeticItems == null)
            //{
            //    AddInitialActiveCosmeticItems(user);
            //    update = update.Set(x => x.ActiveCosmeticItems, user.ActiveCosmeticItems);
            //    needUpdate = true;
            //}

            //if (user.ActiveCosmeticItems.Count == 0)
            //{
            //    AddInitialActiveCosmeticItems(user);
            //    update = update.Set(x => x.ActiveCosmeticItems, user.ActiveCosmeticItems);
            //    needUpdate = true;
            //}

            // daub chest
            //if (user.DaubChest == null)
            //{
            //    AddInitialDaubChest(user);
            //    update = update.Set(x => x.DaubChest, user.DaubChest);
            //    needUpdate = true;
            //}

            // refereal code
            //// only check for referral code in Iran release
            //if (user.ReferralCode == null && !isGlobalRelease)
            //{
            //    await AddReferralCode(user);
            //    update = update.Set(x => x.ReferralCode, user.ReferralCode);
            //    needUpdate = true;
            //}

            //// set ab class
            //if (!user.ABClass.HasValue)
            //{
            //    // set a class for this user
            //    user.ABClass = ABClass.A; //(ABClass)(random.Next(0,2) % 2);
            //    update = update.Set(x => x.ABClass, user.ABClass);
            //    needUpdate = true;
            //}
            //else
            //{
            //    // put all users to A type
            //    if (user.ABClass != ABClass.A)
            //    {
            //        user.ABClass = ABClass.A;
            //        update = update.Set(x => x.ABClass, user.ABClass);
            //        needUpdate = true;
            //    }
            //}

            //// daily reward
            //if (user.LastClaimedDailyReward == null)
            //{
            //    user.LastClaimedDailyReward = DateTime.UtcNow.AddDays(-1);
            //    update = update.Set(x => x.LastClaimedDailyReward, user.LastClaimedDailyReward);
            //    needUpdate = true;
            //}

            //// check userleagues
            //if (user.UserLeagues != null)
            //{
            //    var result = await leagueService.CheckAndUpdateUserLeagues(user.UserLeagues);
            //    if (result)
            //    {
            //        update = update.Set(X => X.UserLeagues, user.UserLeagues);
            //        needUpdate = true;
            //    }
            //}

            //// fortune wheel
            //if (user.FortuneWheel == null)
            //{
            //    await fortuneWheelService.AddInitialFortuneWheel(user);
            //    update = update.Set(x => x.FortuneWheel, user.FortuneWheel);
            //    needUpdate = true;
            //}

            //// clear fortune wheel reward bags
            //if (await fortuneWheelService.CleanRewardBagsDontSave(user))
            //{
            //    update = update.Set(x => x.FortuneWheel, user.FortuneWheel);
            //    needUpdate = true;
            //}

            //// check storeType
            //if (!string.IsNullOrWhiteSpace(storeType) && user.ActiveStore != storeType)
            //{
            //    user.ActiveStore = storeType;
            //    update = update.Set(x => x.ActiveStore, user.ActiveStore);
            //    needUpdate = true;
            //}

            //// update client version
            //if (!string.IsNullOrWhiteSpace(clientVersion) && user.ClientVersion != clientVersion)
            //{
            //    user.ClientVersion = clientVersion;
            //    update = update.Set(x => x.ClientVersion, user.ClientVersion);
            //    needUpdate = true;
            //}

            //// set locale if it is null
            //if (string.IsNullOrWhiteSpace(user.Locale))
            //{
            //    user.Locale = defaultLocale;
            //    update = update.Set(x => x.Locale, user.Locale);
            //    needUpdate = true;
            //}

            //// set referral data
            //if (isGlobalRelease && user.ReferralData == null)
            //{
            //    user.ReferralData = new ReferralData()
            //    {
            //        Step = 0,
            //        CurrentInvitedInStep = 0,
            //        InvitedFriendsToClaim = 0,
            //        RewardChestToClaim = 0
            //    };
            //    update = update.Set(x => x.ReferralData, user.ReferralData);
            //    needUpdate = true;
            //}
            //// add privacy settings
            //if (user.PrivacySettings == null)
            //{
            //    user.PrivacySettings = new PrivacySettings();
            //    update = update.Set(x => x.PrivacySettings, user.PrivacySettings);
            //    needUpdate = true;
            //}

            //// update daily rewards if needed
            //(needUpdate, update) = await CheckUpdateDailyRewards(user, needUpdate, update);

            //// special offer
            //(needUpdate, update) = await specialOfferService.MigrateUserData(user, needUpdate, update);

            //// check events
            //await eventDataMigrationService.MigrateData(user);

            //// check fun fairs
            //await funFairDataMigrationService.MigrateData(user);

            // quests
            // I have put quest updates after event data migration to support eventQuests
            //if (user.QuestSlots == null)
            //{
            //    var questNeedUpdate = await questService.UpdateQuestSlotsDontSaveInDb(user, DefaultNumberOfSlots);
            //    needUpdate = needUpdate || questNeedUpdate;
            //    update = update
            //        .Set(x => x.QuestSlots, user.QuestSlots)
            //        .Set(x => x.QuestsQueue, user.QuestsQueue)
            //        .Set(x => x.EventQuestSlots, user.EventQuestSlots)
            //        .Set(x => x.TimedQuests, user.TimedQuests)
            //        .Set(x => x.TimedQuestsQueue, user.TimedQuestsQueue)
            //        .Set(x => x.ThemeRoomQuests, user.ThemeRoomQuests)
            //        .Set(x => x.ThemeRoomQuestsQueue, user.ThemeRoomQuestsQueue);
            //}

            //// update daily deals
            //(needUpdate, update) = await dailyDealService.UpdateDealsDontSave(user, needUpdate, update);

            //// update special offers
            //(needUpdate, update) = await specialOfferService.UpdateSpecialOfferIfNeededDontSave(user, needUpdate, update);

            //// update tournament Score
            //(needUpdate, update) = await tournamentService.UpdateUserHistories(user, needUpdate, update);

            //// update event quests
            //(needUpdate, update) = await eventQuestManager.UpdateEventQuests(user, needUpdate, update);

            //// update Timed quests
            //(needUpdate, update) = await timedQuestService.UpdateTimedQuests(user, needUpdate, update);

            //// update ThemeRoom quests
            //(needUpdate, update) = await themeRoomQuestService.UpdateThemeRoomQuests(user, needUpdate, update);

            //// update daub chest staus
            //(needUpdate, update) = daubChestService.UpdateDaubChestStatus(user, needUpdate, update);

            return (needUpdate, update);
        }
        //public static void AddInitialFriendsItems(User user)
        //{
        //    user.FriendRequestsReceived = new List<FriendRequest>();
        //    user.Friends = new List<string>();
        //}
    }
}
