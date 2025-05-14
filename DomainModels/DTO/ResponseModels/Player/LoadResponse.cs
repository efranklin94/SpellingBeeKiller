using DomainModels.Models.Game;
using DomainModels.Models.IntermediateModels;

namespace DomainModels.DTO.ResponseModels.Player
{
    public class LoadResponse
    {
        public PlayerProfileModel PlayerModel { get; set; }

        public List<ClassicModeData> ClassisModeDataList { get; set; } = new List<ClassicModeData>();
        public List<CoreBeeGameDataDb> CoreBeeGameDataList { get; set; } = new List<CoreBeeGameDataDb>();
        public List<BeeGamesInvitations> BeeGamesInvitations { get; set; } = new List<BeeGamesInvitations>();
        public List<MainGameHistory> MainGameHistoryList { get; set; } = new List<MainGameHistory>();
    }
}
