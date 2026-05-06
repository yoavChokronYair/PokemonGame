using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.User;

namespace PokemonGame.Services.Interfaces
{
    public interface IProfileService
    {
        ProfileDataTree GetFullProfileData(int battlePlayerId);
        void UpdateSetting(int battlePlayerId, string columnName, int value);
        void SetFavoriteTeam(int battlePlayerId, int? teamId);
        List<BattleHistoryPokemon> GetTeamFormattedList(int teamId);
    }

    // ── Replaces the tuple return — cleaner to pass around ───────────────────
    public class ProfileDataTree
    {
        public BattlePlayerData? Player { get; set; }
        public BattlePlayerStatsData Stats { get; set; } = new();
        public BattlePlayerSettingsData Settings { get; set; } = new();
        public List<TeamData> Teams { get; set; } = new();
    }
}
