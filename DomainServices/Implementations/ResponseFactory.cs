using DomainModels.DTO.ResponseModels.Player;
using DomainModels.Models;
using DomainModels.Models.Game;
using Repositories.Implementations;

namespace DomainServices.Implementations
{
    public class ResponseFactory
    {
        private readonly CoreBeeGameRedisRepository coreBeeGameRedisRepository;

        public ResponseFactory(CoreBeeGameRedisRepository coreBeeGameRedisRepository)
        {
            this.coreBeeGameRedisRepository = coreBeeGameRedisRepository;
        }

        public async Task<LoadResponse> PlayerProfileModel(User user)
        {
            var coreGameData = await coreBeeGameRedisRepository.GetAllForUserAsync(user.Id);

            var model = new LoadResponse
            {
                PlayerModel = new PlayerProfileModel
                {
                    UserId = user.Id!,
                    Username = user.Username,
                    Level = user.Level,
                    Coin = user.Coin,
                },

                CoreBeeGameDataList = coreGameData.ToList(),
            };

            return model;
        }
    }
}
