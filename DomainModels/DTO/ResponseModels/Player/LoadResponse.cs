using DomainModels.Models;
using DomainModels.Models.IntermediateModels;

namespace DomainModels.DTO.ResponseModels.Player
{
    public class LoadResponse
    {
        public PlayerProfileModel PlayerModel { get; set; }

        public List<ClassicModeData> ClassisModeDataList { get; set; } = new List<ClassicModeData>();
        public List<CoreBeeGameData> CoreBeeGameDataList { get; set; } = new List<CoreBeeGameData>();
        public List<BeeGamesInvitations> BeeGamesInvitations { get; set; } = new List<BeeGamesInvitations>();
        public List<MainGameHistory> MainGameHistoryList { get; set; } = new List<MainGameHistory>();
    }
}
