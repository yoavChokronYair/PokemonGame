using PokemonGame.Model.Enums;

namespace PokemonGame.ViewModels.Store
{
    public class UserStore
    {
        public string Username { get; set; }
        public int BattlePlayerID { get; set; }
        public BattleSesion BattleSesion { get; set; }
    }

    public enum BattleMode
    {
        halfTeam,
        TwoThirdsTeam,
        fullTeam
    }

    public enum BotDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public class BattleSesion
    {
        public bool IsOnlineMode { get; set; } = false;
        public bool IsOneVOne { get; set; } = false;
        public BattleMode BattleMode { get; set; } = BattleMode.fullTeam;
        public int? SelectedTeamId { get; set; }
        public List<int> SelectedPokemonIds { get; set; } = new();
        public BotDifficulty BotDifficulty { get; set; } = BotDifficulty.Medium;
    }
}