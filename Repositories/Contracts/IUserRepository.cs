using DomainModels.Models;
using DomainModels.Models.IntermediateModels;
using MongoDB.Driver;

namespace Repositories.Contracts
{
    public interface IUserRepository
    {
        public Task InsertNewUserAsync(User user);
        public Task<User> GetUserByIdAsync(string id);
        public Task<List<User>> GetListOfUsersByIdAsync(List<string> ids);
        public Task<UserBaseModel> GetUserBaseModelByIdAsync(string id);
        public Task<UpdateResult> UpdateUserByModelAsync(User user, UpdateDefinition<User> updateDefinition);
        public Task<UpdateResult> UpdateUserByIdAsync(string userId, UpdateDefinition<User> updateDefinition);
        public Task<User> GetUserByUsernameAsync(string username);
        public Task<User> GetUserByDeviceIdAsync(string deviceId);
        public Task<User> GetUserByReferralCodeAsync(string code);
        public Task<User> GetRecentUserByDeviceIdAsync(string deviceId);
        public Task<List<User>> GetAllUsersByDeviceIdAsync(string deviceId);
        public Task<User> GetUserByEmailAsync(string email);
        public Task<List<User>> GetAllUsersAsync();
        public Task<List<User>> GetListOfPlayersByIdsAsync(List<string> userIds);
        public Task<List<User>> GetListOfPlayersByUsernamesAsync(List<string> userNames);
        public Task<List<User>> FindUsersByRegexAsync(string regex, string options = "i", int limit = 5);
        public Task BulkUpdateUsersAsync(Dictionary<string, UpdateDefinition<User>> updateDictionary);
        public Task<long> GetTotalUsersCountAsync();
        public Task<List<User>> GetRandomUsersAsync(int randomNumber);
        public Task<List<string>> GetListOfPlayersUserNameByUserIdsAsync(List<string> userIds);
        //TODO
        //public Task<List<Friend>> GetFriends(User user);
    }
}
