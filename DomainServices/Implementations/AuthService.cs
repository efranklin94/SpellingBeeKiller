using DomainModels.Models;
using DomainModels.Models.IntermediateModels;
using DomainServices.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using RedisTools.Cache;
using Repositories;
using Repositories.Contracts;
using SharedTools.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace DomainServices.Implementations.UserServices;

public class AuthService : IAuthService
{
    private readonly Random random;
    private readonly IUserRepository userRepository;
    private readonly SharedCacheRedis CacheRedisDb;

    private const string RecoveryIdToUserIdHash = "RecoveryHash";
    private const string VerifyCodeToEmailHash = "VerifyEmailHash";
    private const string RecoveryByDeviceIdExtraString = "SalamAminManClientKhoobiHastam";

    private readonly double ACCESS_TOKEN_EXPIRATION_IN_MINUTES;
    private readonly double REFRESH_TOKEN_EXPIRATION_IN_DAYS;
    private readonly string PREVIOUSLY_RECOVERED_ACOOUNTS_SET;

    private const int SaltCharactersCount = 10;

    private readonly string EncryptionKey;

    public AuthService(IRedisConnection redisConnection, IConfiguration configuration, IUserRepository userRepository)
    {
        this.random = new Random();
        this.userRepository = userRepository;
        this.CacheRedisDb = redisConnection.GetSharedCacheRedis();

        ACCESS_TOKEN_EXPIRATION_IN_MINUTES = double.Parse(configuration.GetSection("ProjectSettings")["ACCESS_TOKEN_EXPIRATION_IN_MINUTES"]!);
        REFRESH_TOKEN_EXPIRATION_IN_DAYS = double.Parse(configuration.GetSection("ProjectSettings")["REFRESH_TOKEN_EXPIRATION_IN_DAYS"]!);
        PREVIOUSLY_RECOVERED_ACOOUNTS_SET = configuration.GetSection("ProjectSettings")["PREVIOUSLY_RECOVERED_ACOOUNTS_SET"]!;

        EncryptionKey = configuration.GetSection("ProjectSettings")["AuthSecert"]!;
    }

    public string GenerateJWTToken(Claim[] claimes, TimeSpan Expiry)
    {
        // authentication successful so generate jwt token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(EncryptionKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claimes),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.Add(Expiry),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<int> GenerateRecoveryIdForUserAsync(string userId)
    {
        Random random = new Random();
        int code = random.Next(10000, 99999);
        int tries = 0;
        while (await CacheRedisDb.HashExistsAsync(RecoveryIdToUserIdHash, code.ToString()))
        {
            code = random.Next(10000, 99990);
            tries++;
            if (tries > 100)
            {
                throw new Exception("failed to recovery code for users");
            }
        }
        await CacheRedisDb.HashSetAsync(RecoveryIdToUserIdHash, code.ToString(), userId,
            TimeSpan.FromMinutes(5));

        return code;
    }

    public async Task<string> GenerateRefreshTokenAsync(string deviceId, string userId)
    {
        var expireAt = DateTime.UtcNow.AddDays(REFRESH_TOKEN_EXPIRATION_IN_DAYS);

        var refreshToken = new RefreshToken
        {
            ExpireAt = expireAt,
            DeviceId = deviceId,
            RandomNounce = random.Next(10, 10000000),
            UserId = userId
        };

        var rToken = EncryptionMethods.Toolbox.Encrypt(JsonSerializer.Serialize(refreshToken), EncryptionKey, SaltCharactersCount);

        //save user requested to generate refresh token
        await CacheRedisDb.SetAddAsync(PREVIOUSLY_RECOVERED_ACOOUNTS_SET, userId, TimeSpan.FromDays(1));

        return rToken!;
    }

    public async Task<int> GenerateVerifyCodeForEmailAsync(string email)
    {
        Random random = new Random();
        int code = random.Next(10000, 99999);
        int tries = 0;
        while (await CacheRedisDb.HashExistsAsync(VerifyCodeToEmailHash, code.ToString()))
        {
            code = random.Next(10000, 99990);
            tries++;
            if (tries > 100)
            {
                throw new Exception($"failed to generate email verify code for email : {email}");
            }
        }
        await CacheRedisDb.HashSetAsync(VerifyCodeToEmailHash, code.ToString(), email,
            TimeSpan.FromMinutes(5));

        return code;
    }

    public async Task<(string token, string error)> GetAccessTokenAsync(string refreshToken)
    {
        // validate Refresh Token
        (bool isValid, RefreshToken RToken) = ValidateRefreshToken(refreshToken);
        if (!isValid)
        {
            return (null!, CustomMessages.RefreshTokenNotValid);
        }

        var user = await userRepository.GetUserByIdAsync(RToken.UserId);
        if (user == null)
        {
            return (null!, CustomMessages.UserNotFound);
        }

        if (!string.IsNullOrWhiteSpace(user.RefreshToken))
        {
            // check if current user refresh token is the same that user sent us- to prevent using one account on two devices or more
            if (user.RefreshToken != refreshToken)
            {
                return (null!, CustomMessages.RefreshTokenNotValid);
            }
        }
        else
        {
            // update user refersh token
            user.RefreshToken = refreshToken;
            await userRepository.UpdateUserByModelAsync(user, Builders<User>.Update.Set(x => x.RefreshToken, refreshToken));
        }

        // authentication successful so generate jwt token
        var claims = new Claim[]
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier , user.Id.ToString()),
            new Claim(ClaimTypes.Hash, EncryptionMethods.Toolbox.ComputeSha256Hash(refreshToken)),
            new Claim(ClaimTypes.SerialNumber, RToken.DeviceId)
        };

        var Expiry = TimeSpan.FromMinutes(ACCESS_TOKEN_EXPIRATION_IN_MINUTES);

        var tokenString = GenerateJWTToken(claims, Expiry);
        return (tokenString, null!);
    }

    private (bool IsValid, RefreshToken token) ValidateRefreshToken(string refreshToken)
    {
        string refreshTokenJson = "";
        try
        {
            refreshTokenJson = EncryptionMethods.Toolbox.Decrypt(refreshToken, EncryptionKey, SaltCharactersCount);
        }
        catch (Exception)
        {
            return (false, null!);
        }

        RefreshToken Token = JsonSerializer.Deserialize<RefreshToken>(refreshTokenJson)!;
        if (Token == null)
        {
            return (false, null!);
        }

        if (Token.ExpireAt > DateTime.UtcNow)
        {
            // token is valid
            return (true, Token);
        }
        else
        {
            return (false, null!);
        }
    }

    public string GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(EncryptionKey);
        if (!tokenHandler.CanReadToken(token))
        {
            return null!;
        }

        try
        {
            var tk = tokenHandler.ReadJwtToken(token);
            return tk.Claims.FirstOrDefault(x => x.Type == "nameid")!.Value;
        }
        catch (Exception)
        {
            return null!;
        }
    }

    public async Task<(User user, string error)> RecoverAccountByDeviceIdAsync(string deviceId, string proof)
    {
        var result = await UserExistsWithDeviceIdAsync(deviceId);
        if (!result.success)
        {
            return (null!, CustomMessages.UserNotExist);
        }

        //check proof
        var isValid = ValidateRecoveryByDeviceIdProof(deviceId, proof);
        if (!isValid)
        {
            return (null!, CustomMessages.ItemIsNotValid("Proof"));
        }

        // commented, reason: we should not limit users when they are recovering by device
        // if (await CacheRedisDb.SetContainsAsync(PREVIOUSLY_RECOVERED_ACOOUNTS_SET, result.user.Id))
        // {
        //     return (null, "You have reached your recovery limit");
        // }

        await RenewRefreshTokenAsync(result.user, deviceId);

        return (result.user, null!);
    }

    private bool ValidateRecoveryByDeviceIdProof(string deviceId, string userProof)
    {
        try
        {
            var decrypted = EncryptionMethods.Toolbox.Decrypt(userProof, RecoveryByDeviceIdExtraString, 3);

            var pr = JsonSerializer.Deserialize<RecoverByDeviceProof>(decrypted);
            if (pr?.DeviceId == deviceId)
            {
                return true;
            }
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string> RenewRefreshTokenAsync(User user, string deviceId)
    {
        var rToken = await GenerateRefreshTokenAsync(deviceId, user.Id);
        user.RefreshToken = rToken;
        await userRepository.UpdateUserByModelAsync(user, Builders<User>.Update.Set(x => x.RefreshToken, rToken));
        return rToken;
    }

    public async Task<(bool success, string error)> SetEmailAsync(string newEmail)
    {
        // check if email is repetitive
        var oldUser = await userRepository.GetUserByEmailAsync(newEmail);
        if (oldUser != null)
        {
            return (false, CustomMessages.ItemAlreadyExists("Email"));
        }

        // create a new code and send email
        var code = await GenerateVerifyCodeForEmailAsync(newEmail);

        // TODO: we dont implemented mailServices yet!
        // BackgroundJob.Enqueue(() =>
        //     // send email code
        //     mailServices.SendEmail("Player", newEmail, EmailSubjectText,
        //         string.Format(VerifyEmailText, code))
        // );

        return (true, null!);
    }

    public async Task<(bool success, User user)> UserExistsWithDeviceIdAsync(string deviceId)
    {
        var user = await userRepository.GetRecentUserByDeviceIdAsync(deviceId);
        if (user != null)
        {
            return (true, user);
        }
        return (false, null!);
    }

    public async Task<(bool sucess, string error)> VerifyEmailAsync(string userId, string code, string email)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return (false, "Code is empty");
        }

        if (await CacheRedisDb.HashExistsAsync(VerifyCodeToEmailHash, code.ToString()))
        {
            string storedEmail = await CacheRedisDb.HashGetAsync(VerifyCodeToEmailHash, code.ToString());
            if (storedEmail != email)
            {
                return (false, "Code expired");
            }

            var user = await userRepository.GetUserByIdAsync(userId);
            user.Email = storedEmail;

            var update = Builders<User>.Update
                .Set(x => x.Email, user.Email);

            await userRepository.UpdateUserByModelAsync(user, update);

            //clear code from cache
            await CacheRedisDb.HashRemoveAsync(VerifyCodeToEmailHash, code);

            return (true, null!);
        }

        return (false, "Code expired");
    }

    public class RecoverByDeviceProof
    {
        public required string DeviceId { get; set; }
        public required string RandomNounce { get; set; }
    }
}
