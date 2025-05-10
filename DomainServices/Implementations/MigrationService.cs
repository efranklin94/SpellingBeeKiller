using DomainModels.Models;
using MongoDB.Driver;
using Repositories.Contracts;

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

        public async Task<(bool needUpdate, UpdateDefinition<User> updateDefinition)> MigrateAndUpdateUserData(User user, string storeType, string clientVersion)
        {
            var update = Builders<User>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow);
            bool needUpdate = false;

            //tournaments History
            //if (user.TournamentsHistory == null)
            //{
            //    user.TournamentsHistory = new List<UserTournaments>(TournamentHistoryQueueSize);
            //    update = update.Set(x => x.TournamentsHistory, user.TournamentsHistory);
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

            //// update tournament Score
            //(needUpdate, update) = await tournamentService.UpdateUserHistories(user, needUpdate, update);

            return (needUpdate, update);
        }
    }
}
