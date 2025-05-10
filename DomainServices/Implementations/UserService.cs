using DomainModels.Models;
using DomainModels.Models.IntermediateModels;
using DomainServices.Contracts.UserServices;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using RedisTools.Cache;
using Repositories;
using Repositories.Contracts;
using SharedTools.Tools;

namespace DomainServices.Implementations.UserServices;

public class UserService : IUserService
{
    private readonly byte ChangeUsernameCost = 1; // TODO
    private readonly string PREVIOUSLY_USERNAME_CHANGED_SET;
    private readonly string defaultLocale;

    private readonly MigrationService migrationService;
    private readonly IUserRepository userRepository;
    //private readonly IFlaggedUsersService flaggedUsersService;
    //private readonly IUserPublicProfileService userPublicProfileService;

    private readonly SharedCacheRedis CacheRedisDb;

    public UserService(IConfiguration configuration, IUserRepository userRepository, MigrationService migrationService,
        IRedisConnection redisConnection
        //,IUserPublicProfileService userPublicProfileService, IFlaggedUsersService flaggedUsersService
        )
    {
        this.userRepository = userRepository;
        this.migrationService = migrationService;
        //this.userPublicProfileService = userPublicProfileService;
        //this.flaggedUsersService = flaggedUsersService;

        defaultLocale = configuration["ProjectSettings:defaultLocale"]!;

        PREVIOUSLY_USERNAME_CHANGED_SET = configuration.GetSection("ProjectSettings")["PREVIOUSLY_USERNAME_CHANGED_SET"]!;
        CacheRedisDb = redisConnection.GetSharedCacheRedis();
    }

    public async Task<(UserBaseModel UserBaseModel, string Error)> GetUserBaseModel(string userId)
    {
        var playerProgressModel = await userRepository.GetUserBaseModelByIdAsync(userId);
        if (playerProgressModel == null)
        {
            return (null!, CustomMessages.UserNotFound);
        }
        return (playerProgressModel, null!);
    }

    public async Task<(User user, string error)> CreateUserAsync(string deviceId, string username = null!,
        bool isEmulator = false, string locale = null!)
    {
        // check for banned devices
        //if (await flaggedUsersService.IsDeviceIdBannedAsync(deviceId))
        //{
        //    return (null!, "You are banned");
        //}

        // check if user has an account before do not create new one
        var users = await userRepository.GetAllUsersByDeviceIdAsync(deviceId);
        if (users.Count > 2)
        {
            return (users[users.Count - 1], null!);
        }

        if (username == null)
        {
            username = CreateRandomUsername();
        }

        // set default locale if not set
        if (string.IsNullOrWhiteSpace(locale))
        {
            locale = defaultLocale;
        }

        var user = new User()
        {
            Level = 1,
            Username = username,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ActiveDeviceId = deviceId,
            Ticket = 200,
            Coin = 200,
            IsEmulator = isEmulator,
            //TODO: Locale = locale
        };

        // TODO
        // change ticket/coin rewards for different release modes

        // TODO handle ABClass

        // TODO add initial data
        // add initial powerups
        // add initial FriendsItems
        //MigrationService.AddInitialCosmeticItems(user);
        //MigrationService.AddInitialStickers(user);
        // add initial daub chest

        // TODO
        //var releaseModeGlobal = Environment.GetEnvironmentVariable("RELEASE_MODE_GLOBAL");
        //if (string.IsNullOrWhiteSpace(releaseModeGlobal))
        //{
        //    // Iran release
        //    // we do not have referral code in global release
        //    await migrationService.AddReferralCode(user);
        //}

        // TODO
        // add initial will fortune
        // add starter pack

        // TODO
        // set these times to future, later when user finished tutorial we will set these timers
        //user.FortuneWheel.LastClaimedFreeReward = DateTime.UtcNow.AddYears(20);
        //user.LastClaimedDailyReward = DateTime.UtcNow.AddYears(20);

        // TODO
        //UpdateQuestSlots

        await userRepository.InsertNewUserAsync(user);

        // TODO
        //metrics.Measure.Counter.Increment(totalRegisters);

        return (user, null!);
    }

    //public async Task<(User user, string error)> ChangeUserNameAsync(string userId, string newUsername)
    //{
    //    newUsername = newUsername.FixPersian();

    //    if (await CacheRedisDb.SetContainsAsync(PREVIOUSLY_USERNAME_CHANGED_SET, userId))
    //    {
    //        return (null!, CustomMessages.LimitReached);
    //    }

    //    if (newUsername.Length < 4)
    //    {
    //        return (null!, CustomMessages.LengthShoulBeGreaterThan(4));
    //    }

    //    if (newUsername.Length > 15)
    //    {
    //        return (null!, CustomMessages.LengthShoulBeGreaterThan(15));
    //    }

    //    if (!ToolBax.ValidateUserName(newUsername))
    //    {
    //        return (null!, $"new {CustomMessages.ItemIsNotValid("UserName")}");
    //    }

    //    if (!string.IsNullOrWhiteSpace(newUsername) && !NameCheckService.IsNameLegit(newUsername))
    //    {
    //        return (null!, "user proper usernames");
    //    }

    //    var user = await userRepository.GetUserByIdAsync(userId);
    //    if (user == null)
    //    {
    //        return (null!, CustomMessages.UserNotFound);
    //    }

    //    // reduce some coins if its not free
    //    if (user.UserHasChangedUsername)
    //    {
    //        if (user.Coin < ChangeUsernameCost)
    //        {
    //            return (null!, CustomMessages.DoesNotHaveEnoughItems("coins"));
    //        }
    //        user.Coin -= ChangeUsernameCost;
    //    }

    //    // check if username is available
    //    var oldUser = await userRepository.GetUserByUsernameAsync(newUsername);
    //    if (oldUser != null)
    //    {
    //        return (null!, CustomMessages.UserNameNotAvailable);
    //    }

    //    // store previous username
    //    user.PreviousUsernames ??= new List<string>();
    //    user.PreviousUsernames.Add(user.UserName);
    //    user.UserHasChangedUsername = true;
    //    user.UserName = newUsername;

    //    var update = Builders<User>.Update
    //        .Set(x => x.UserName, user.UserName)
    //        .Set(x => x.UserHasChangedUsername, true)
    //        .Set(x => x.Coin, user.Coin)
    //        .Set(x => x.PreviousUsernames, user.PreviousUsernames);

    //    var result = await userRepository.UpdateUserByModelAsync(user, update);

    //    await userPublicProfileService.ClearCacheAsync(user.Id);

    //    // TODO push to clan members
    //    //if (user.Clan != null)
    //    //{
    //    //    hubService.PushMessageToClan(user.Clan.ClanId, new ClanMemberUsernameChanged
    //    //    {
    //    //        EventType = BaseEvent.EventTypes.ClanMemberUsernameChanged,
    //    //        UserId = user.Id,
    //    //        NewUsername = user.Username
    //    //    });
    //    //}

    //    // save the user request for renaming its username for 24hours
    //    await CacheRedisDb.SetAddAsync(PREVIOUSLY_USERNAME_CHANGED_SET, userId, TimeSpan.FromDays(1));

    //    return (user, null!);
    //}

    //public async Task<(User user, string error)> ChangeEmailAsync(string userId, string newEmail)
    //{
    //    var user = await userRepository.GetUserByIdAsync(userId);
    //    if (user == null)
    //    {
    //        return (null!, CustomMessages.UserNotFound);
    //    }

    //    // check if email is repetitive
    //    var oldUser = await userRepository.GetUserByEmailAsync(newEmail);
    //    if (oldUser != null)
    //    {
    //        return (null!, CustomMessages.ItemAlreadyExists("email"));
    //    }

    //    user.Email = newEmail;
    //    var update = Builders<User>.Update.Set(x => x.Email, user.Email);

    //    var result = await userRepository.UpdateUserByModelAsync(user, update);

    //    return (user, null!);
    //}

    //public async Task<(List<string> activeItems, string error)> UpdateCosmeticItemsAsync(string userId,
    //    List<string> itemsToActivate)
    //{
    //    var user = await userRepository.GetUserByIdAsync(userId);

    //    user.ActiveCosmeticItems ??= new List<string>();
    //    user.CosmeticInventory ??= new List<string>();

    //    foreach (var item in itemsToActivate)
    //    {
    //        if (!user.CosmeticInventory.Contains(item))
    //        {
    //            return (null!, $"You can not active items that you do not own: {item}");
    //        }
    //    }

    //    user.ActiveCosmeticItems = itemsToActivate;

    //    // todo validate cosmetics for example check if user does not activated two faces or two hairs

    //    var update = Builders<User>.Update.Set(x => x.ActiveCosmeticItems, user.ActiveCosmeticItems);
    //    await userRepository.UpdateUserByModelAsync(user, update);

    //    // clear user profile cache
    //    await userPublicProfileService.ClearCacheAsync(user.Id);

    //    return (user.ActiveCosmeticItems, null!);
    //}

    private string CreateRandomUsername()
    {

        string createUsername()
        {
            var releaseModeGlobal = Environment.GetEnvironmentVariable("RELEASE_MODE_GLOBAL");
            if (string.IsNullOrWhiteSpace(releaseModeGlobal))
            {
                // Iran
                return "بازیکن" + Nanoid.Nanoid.Generate("1234567890abcdefghijklmopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", 7) + "-";
            }
            else
            {
                // Global
                return "Guest-" + Nanoid.Nanoid.Generate("1234567890abcdefghijklmopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", 7);
            }
        }

        var username = createUsername();

        var badUsernames = new List<bool>()
            {
                username.Contains("sex", StringComparison.CurrentCultureIgnoreCase),
                username.Contains("cos", StringComparison.CurrentCultureIgnoreCase),
                username.Contains("kos", StringComparison.CurrentCultureIgnoreCase),
                username.Contains("kir", StringComparison.CurrentCultureIgnoreCase),
            };

        int tries = 0;
        while (badUsernames.Any())
        {
            username = createUsername();
            tries++;
            if (tries > 5)
            {
                break;
            }
        }

        return username;
    }

    public static List<string> CreateRandomCosmeticItems()
    {
        var list = new List<string>();
        Random random = new Random();

        int specialItem = random.Next(0, 3);

        if (specialItem == 0)
        {
            list.Add($"hair{random.Next(9, 41)}");
        }
        else
        {
            list.Add($"hair{random.Next(0, 9)}");
        }

        list.Add($"skin{random.Next(0, 4)}");

        if (specialItem == 1)
        {
            list.Add($"eye{random.Next(5, 21)}");
        }
        else
        {
            list.Add($"eye{random.Next(0, 5)}");
        }

        list.Add($"mouth{random.Next(0, 8)}");

        var releaseModeGlobal = Environment.GetEnvironmentVariable("RELEASE_MODE_GLOBAL");
        if (string.IsNullOrWhiteSpace(releaseModeGlobal))
        {
            // IRAN
            switch (specialItem)
            {
                case 0:
                    list.Add($"daub0");
                    break;
                case 1:
                    list.Add($"daub{random.Next(3, 15)}");
                    break;
                case 2:
                    list.Add($"daub{random.Next(15, 20)}");
                    break;
                default:
                    list.Add($"daub{random.Next(0, 3)}");
                    break;
            }

        }
        else
        {
            // GLOBAL
            if (specialItem == 2)
            {
                list.Add($"daub{random.Next(0, 20)}");
            }
            else
            {
                list.Add($"daubdefault");
            }
        }


        if (specialItem == 2)
        {
            list.Add($"acc{random.Next(0, 22)}");
        }
        else
        {
            list.Add("acc");
        }

        return list;
    }
}