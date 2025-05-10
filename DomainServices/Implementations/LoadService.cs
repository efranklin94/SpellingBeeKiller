using DomainModels.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Repositories;
using Repositories.Contracts;

namespace DomainServices.Implementations.UserServices
{
    public class LoadService
    {
        private readonly IUserRepository userRepository;

        private readonly IDatabaseContext database;

        private readonly MigrationService migrationService;

        private readonly ILogger<LoadService> logger;


        public LoadService(IConfiguration configuration, IDatabaseContext databaseContext, IUserRepository userRepository, ILogger<LoadService> logger, MigrationService migrationService)
        {
            this.logger = logger;
            this.userRepository = userRepository;
            this.database = databaseContext;
            this.migrationService = migrationService;
        }

        public async Task<(User User, string error)> Load(string userId, string clientVersion = "0.0.1" , string deviceId = "string",
            string refreshTokenHash = null, string storeType = null)
        {
            var user = await userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return (null, "User Not Found!");
            }
            
            //if (!string.IsNullOrWhiteSpace(user.RefreshToken))
            //{
            //    // validate access token is issued by the right refresh token 
            //    if (refreshTokenHash != EncryptionMethods.Toolbox.ComputeSha256Hash(user.RefreshToken))
            //    {
            //        // this happens when user restore his account in an other device, the old device should get this error
            //        return (null, "access token issued by wrong Refresh token");
            //    }
            //}

            // migrate user if needed
            (bool needUpdate, UpdateDefinition<User> update) = await migrationService.MigrateAndUpdateUserData(user, storeType, clientVersion);

            if (needUpdate)
            {
                // update user
                await userRepository.UpdateUserByModelAsync(user, update);
            }

            logger.LogInformation("user loads");

            return (user, null);
        }

    }

}
