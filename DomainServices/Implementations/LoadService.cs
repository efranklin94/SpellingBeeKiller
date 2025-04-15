using DomainModels.Models;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Repositories.Implementations;
using Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Contracts;
using DomainServices.Contracts.UserServices;

namespace DomainServices.Implementations.UserServices
{
    public class LoadService
    {
        private readonly IUserRepository userRepository;

        private readonly IDatabaseContext database;

        private readonly MigrationService migrationService;

        private readonly ILogger<LoadService> logger;
        //private readonly IFlaggedUsersService flaggedUsersService;
        //private readonly CardBoostService cardBoostService;
        //private readonly MaintenanceService maintenanceService;
        //private readonly ShopService shopService;
        //private readonly BazaarPayService bazaarPayService;
        //private readonly IMetrics metrics;

        //private static CounterOptions LoadGameCountTotal => new CounterOptions
        //{
        //    Name = "load_game_count_total"
        //};

        //private static MeterOptions ActiveUsersStore => new MeterOptions
        //{
        //    Name = "active_users_store",
        //    MeasurementUnit = Unit.Calls
        //};


        //private static MeterOptions ActiveUserCountry => new MeterOptions
        //{
        //    Name = "active_users_country",
        //    MeasurementUnit = Unit.Calls
        //};

        //private static MeterOptions ActiveUsersClientVersion => new MeterOptions
        //{
        //    Name = "active_users_client_version",
        //    MeasurementUnit = Unit.Calls
        //};

        //private static MeterOptions ActiveUsersClientVersionWithStore => new MeterOptions
        //{
        //    Name = "active_users_client_version_with_store",
        //    MeasurementUnit = Unit.Calls
        //};


        private readonly string MaxAcceptableVersion;


        public LoadService(IConfiguration configuration, IDatabaseContext databaseContext, 
            IUserRepository userRepository, ILogger<LoadService> logger, 
            //IFlaggedUsersService flaggedUsersService,
            //MaintenanceService maintenanceService, CardBoostService cardBoostService, IMetrics metrics, ShopService shopService, BazaarPayService bazaarPayService,
            MigrationService migrationService)
        {
            this.logger = logger;
            this.userRepository = userRepository;

            this.database = databaseContext;

            this.migrationService = migrationService;
            //this.flaggedUsersService = flaggedUsersService;
            //this.metrics = metrics;
            //this.cardBoostService = cardBoostService;
            //this.maintenanceService = maintenanceService;
            //this.shopService = shopService;
            //this.bazaarPayService = bazaarPayService;

            //MaxAcceptableVersion = configuration.GetSection("ProjectSettings")["MaxAcceptableVersion"];
        }



        public async Task<(User User, string error)> Load(string clientVersion, string userId, string deviceId,
            string refreshTokenHash, string storeType)
        {
            //TODO
            //check for version
            //var versionCheck = CheckClientVersion(clientVersion);

            //if (!versionCheck.success)
            //{
            //    return (null, $"Minimum Version is {versionCheck.MinimumVersion}, update your client!");
            //}

            //TODO
            // check for Maintenance Mode
            //if (await maintenanceService.CheckMaitenanceModeInLoad(storeType, clientVersion))
            //{
            //    return (null, MaintenanceService.MAINTENANCE_MESSAGE);
            //}

            //check for banned users
            //if (await flaggedUsersService.IsUserBannedAsync(userId))
            //{
            //    return (null, "You are banned");
            //}

            var user = await userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return (null, "User Not Found!");
            }

            // check for banned devices
            //if (await flaggedUsersService.IsDeviceIdBannedAsync(user.ActiveDeviceId))
            //{
            //    return (null, "You are banned");
            //}

            if (!string.IsNullOrWhiteSpace(user.RefreshToken))
            {
                // validate access token is issued by the right refresh token 
                if (refreshTokenHash != EncryptionMethods.Toolbox.ComputeSha256Hash(user.RefreshToken))
                {
                    // this happens when user restore his account in an other device, the old device should get this error
                    return (null, "access token issued by wrong Refresh token");
                }
            }

            //TODO
            //if (user.UpdatedAt + TimeSpan.FromDays(2) < DateTime.UtcNow)
            //{
            //    // user has not came here for more than two days
            //    // lets give him some card boost in games
            //    await cardBoostService.GiveUserGoodCards(userId, 1, 1);
            //}

            // migrate user if needed
            (bool needUpdate, UpdateDefinition<User> update) = await migrationService.MigrateAndUpdateUserData(user, storeType, clientVersion);


            if (needUpdate)
            {
                // update user
                await userRepository.UpdateUserByModelAsync(user, update);
            }

            //TODO
            // add purchased items to users who have paid but not claimed
            //var updatedUser = await shopService.ConsumeUnfinishedZarrinpalPayemnt(user);
            //if (updatedUser != null)
            //{
            //    user = updatedUser;
            //}

            // add purchased items to users who have paid but not claimed
            //var updatedUserBazaarPay = await bazaarPayService.ConsumeUnfinishedPayemnt(user);
            //if (updatedUserBazaarPay != null)
            //{
            //    user = updatedUserBazaarPay;
            //}

            //BackgroundJob.Enqueue<PrivateFriendsGameService>(x => x.CheckUnClosedPrivatRooom(user.Id));

            //metrics.Measure.Counter.Increment(LoadGameCountTotal);

            //metrics.Measure.Meter.Mark(ActiveUsersStore, storeType);

            //if (!string.IsNullOrEmpty(user.Locale))
            //{
            //    metrics.Measure.Meter.Mark(ActiveUserCountry, user.Locale);
            //}
            //metrics.Measure.Meter.Mark(ActiveUsersClientVersion, clientVersion);

            //metrics.Measure.Meter.Mark(ActiveUsersClientVersionWithStore, $"{storeType}_{clientVersion}");

            logger.LogInformation("user loads");

            return (user, null);
        }


        //public (bool success, string MinimumVersion) CheckClientVersion(string clientVersion)
        //{
        //    var result = BingoCardGenerator.Tools
        //        .Toolbox.IsClientVersionBiggerThanServerVersion(MaxAcceptableVersion, clientVersion);

        //    return (result, MaxAcceptableVersion);

        //}
    }

}
