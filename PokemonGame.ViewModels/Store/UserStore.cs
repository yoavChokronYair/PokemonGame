using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Services.Factory;
using PokemonGame.Services.Handler;

namespace PokemonGame.ViewModels.Store
{
    public class UserStore
    {
        // ── Identity ──────────────────────────────────────────────────────────

        public string Username { get; set; }
        public int BattlePlayerID { get; set; }

        // ── Pre-battle session ────────────────────────────────────────────────
        public BattleSession BattleSesion { get; set; } = new();
        public ServiceResolver Resolver { get; set; } = new ServiceResolver(false);
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

    public class BattleSession
    {
        public int? RivalTeamId { get; set; }
        public bool IsOnlineMode { get; set; } = false;
        public bool IsOneVOne { get; set; } = false;
        public BattleMode BattleMode { get; set; } = BattleMode.fullTeam;
        public int? SelectedTeamId { get; set; }
        public List<int> SelectedPokemonIds { get; set; } = new();
        public BotDifficulty BotDifficulty { get; set; } = BotDifficulty.Medium;
        public List<int> RivalPokemonIds { get; set; } = new();


        // ── NEW: resolved before BattleViewModel is created ──────────────────
        public PokemonTeam? ResolvedPlayerTeam { get; set; }
        public PokemonTeam? ResolvedBotTeam { get; set; }
    }
}