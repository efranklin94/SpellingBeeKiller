using DomainModels.Models;
using DomainModels.Models.IntermediateModels;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Repositories.Contracts;

namespace Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseContext database;

        public readonly List<string> BotUsernames;

        public UserRepository(IDatabaseContext databaseContext)
        {
            this.database = databaseContext;
        }

        public async Task InsertNewUserAsync(User user)
        {
            await database.UsersCollection.InsertOneAsync(user);
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await database.UsersCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetListOfUsersByIdAsync(List<string> ids)
        {
            var filter = Builders<User>.Filter.In(u => u.Id, ids);
            return await database.UsersCollection.Find(filter).ToListAsync();
        }

        public async Task<UserBaseModel> GetUserBaseModelByIdAsync(string id)
        {
            return await database.UsersCollection
                .Find(x => x.Id == id)
                .Project(user => new UserBaseModel(user))
                .FirstOrDefaultAsync();
        }

        public virtual async Task<UpdateResult> UpdateUserByModelAsync(User user, UpdateDefinition<User> updateDefinition)
        {
            // TODO updateDefinition = ClearUselessData(user, updateDefinition).Set(x => x.UpdatedAt, DateTime.UtcNow);
            updateDefinition = updateDefinition.Set(x => x.UpdatedAt, DateTime.UtcNow);
            var result = await database.UsersCollection.UpdateOneAsync(x => x.Id == user.Id, updateDefinition);
            return result!;
        }

        public virtual async Task<UpdateResult> UpdateUserByIdAsync(string userId, UpdateDefinition<User> updateDefinition)
        {
            updateDefinition = updateDefinition.Set(x => x.UpdatedAt, DateTime.UtcNow);
            var result = await database.UsersCollection.UpdateOneAsync(x => x.Id == userId, updateDefinition);
            return result!;
        }

        public async Task<User> GetUserByUsernameAsync(string userName)
        {
            return await database.UsersCollection.Find(x => x.Username == userName).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByDeviceIdAsync(string deviceId)
        {
            return await database.UsersCollection.Find(user => user.ActiveDeviceId == deviceId).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByReferralCodeAsync(string code)
        {
            return await database.UsersCollection.Find(x => x.ReferralCode == code).FirstOrDefaultAsync();
        }

        public async Task<User> GetRecentUserByDeviceIdAsync(string deviceId)
        {
            return await database.UsersCollection
                .Find(user => user.ActiveDeviceId == deviceId)
                .SortByDescending(e => e.UpdatedAt)
                .Limit(1)
                .FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAllUsersByDeviceIdAsync(string deviceId)
        {
            return await database.UsersCollection.Find(user => user.ActiveDeviceId == deviceId).ToListAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var filter = Builders<User>.Filter.Eq(x => x.Email, email);
            filter &= Builders<User>.Filter.Type(x => x.Email, BsonType.String);

            return await database.UsersCollection
                .Find(filter, new FindOptions() { Collation = new Collation("en", strength: CollationStrength.Secondary) })
                .FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await database.UsersCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<User>> GetListOfPlayersByIdsAsync(List<string> userIds)
        {
            if (userIds.Count == 0)
            {
                return new List<User>();
            }
            return await database.UsersCollection.Find(user => userIds.Contains(user.Id)).ToListAsync();
        }

        public async Task<List<User>> GetListOfPlayersByUsernamesAsync(List<string> userNames)
        {
            if (userNames.Count == 0)
            {
                return new List<User>();
            }
            return await database.UsersCollection.Find(user => userNames.Contains(user.Username)).ToListAsync();
        }

        public async Task<List<User>> FindUsersByRegexAsync(string regex, string options = "i", int limit = 5)
        {
            try
            {
                var filter = Builders<User>.Filter
                    .Regex(user => user.Username, new BsonRegularExpression(regex, options));

                return await database.UsersCollection.Find(filter).Limit(limit).ToListAsync();
            }
            catch (Exception)
            {
                return new List<User>();
            }
        }

        public async Task BulkUpdateUsersAsync(Dictionary<string, UpdateDefinition<User>> updateDictionary)
        {
            if (updateDictionary.Count == 0)
            {
                return;
            }

            var bulkOps = new List<WriteModel<User>>();

            foreach (var item in updateDictionary)
            {
                var updateUser = new UpdateOneModel<User>(Builders<User>.Filter.Eq(x => x.Id, item.Key), item.Value);
                bulkOps.Add(updateUser);
            }

            //update users
            await database.UsersCollection.BulkWriteAsync(bulkOps);
        }

        public async Task<long> GetTotalUsersCountAsync()
        {
            return await database.UsersCollection.EstimatedDocumentCountAsync();
        }

        public async Task<List<User>> GetRandomUsersAsync(int randomNumber)
        {
            return await database.UsersCollection.AsQueryable().Sample(randomNumber).ToListAsync();
        }

        public async Task<List<string>> GetListOfPlayersUserNameByUserIdsAsync(List<string> userIds)
        {
            var userNames = new List<string>();
            if (userIds.Count == 0)
            {
                return userNames;
            }

            foreach (var userId in userIds)
            {
                var username = await database.UsersCollection.Find(user => user.Id == userId).Project(x => x.Username).FirstOrDefaultAsync();
                userNames.Add(username);
            }

            return userNames;
        }
        //TODO
        //public async Task<List<Friend>> GetFriends(User user)
        //{
        //    if (user.Friends == null)
        //    {
        //        return new List<Friend>();
        //    }

        //    if (user.Friends.Count == 0)
        //    {
        //        return new List<Friend>();
        //    }

        //    var friends = await GetListOfPlayersByIdsAsync(user.Friends);

        //    return friends.Select(x => new Friend(x, user)).ToList();
        //}

    }
}
