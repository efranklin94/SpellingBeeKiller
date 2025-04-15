using DomainModels.Models;
using DomainModels.Models.IntermediateModels;

namespace DomainServices.Contracts.UserServices
{
    public interface IUserService
    {
        public Task<(UserBaseModel UserBaseModel, string Error)> GetUserBaseModel(string userId);
        public Task<(User user, string error)> CreateUserAsync(string deviceId, string username = null!,
            bool isEmulator = false, string locale = null!);
        //public Task<(User user, string error)> ChangeUserNameAsync(string userId, string newUsername);
        //public Task<(User user, string error)> ChangeEmailAsync(string userId, string newEmail);
        //public Task<(List<string> activeItems, string error)> UpdateCosmeticItemsAsync(string userId,
        //    List<string> itemsToActivate);
    }
}
