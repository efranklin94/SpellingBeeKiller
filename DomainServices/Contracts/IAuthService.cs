using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DomainModels.Models;

namespace DomainServices.Contracts
{
    public interface IAuthService // TODO
    {
        // public Task<(bool status, string error)> GetRecove7ryCodeByEmail(string email);
        // public Task<(User oldUser, string error)> ExternalAuth(string userId, string accessToken);
        // public Task<(User oldUser, string error)> LoginWithFacebook(string userId, string accessToken, string activeDeviceId);
        // public Task<(User oldUser, string error)> LoginWithApple(string userId, string authorizationToken, string activeDeviceId, string uniqueId);
        // public Task<(User oldUser, string error)> LoginWithGoogle(string userId, string idToken);
        // public Task SetExternalRecoveryId(string userId, string externalId, ExternalAuthProvider externalAuthProvider);
        // public Task<(User user, string error)> RecoverAccountByRecoveryCode(int recoveryCode, string newDeviceId);
        public Task<(User user, string error)> RecoverAccountByDeviceIdAsync(string deviceId, string proof);
        public Task<(bool success, User user)> UserExistsWithDeviceIdAsync(string deviceId);
        public string GetUserIdFromToken(string token);
        public Task<(string token, string error)> GetAccessTokenAsync(string refreshToken);
        public string GenerateJWTToken(Claim[] claimes, TimeSpan Expiry);
        public Task<string> RenewRefreshTokenAsync(User user, string deviceId);
        public Task<string> GenerateRefreshTokenAsync(string deviceId, string userId);
        public Task<int> GenerateRecoveryIdForUserAsync(string userId);
        public Task<int> GenerateVerifyCodeForEmailAsync(string email);
        public Task<(bool success, string error)> SetEmailAsync(string newEmail);
        public Task<(bool sucess, string error)> VerifyEmailAsync(string userId, string code, string email);
    }
}
