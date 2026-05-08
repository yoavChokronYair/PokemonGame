
using static System.Net.Mime.MediaTypeNames;

namespace PokemonGame.Services.Interfaces
{
    public interface IBattleService
    {
        event Action? OnStateUpdated;

        bool IsOver { get; }
        string? WinnerName { get; }

        void RunTurn(int index, string action = "Move");  // "Move" or "Switch"
        void Forfeit();
        BattleSnapshot GetState();
    }
    public class BattleSnapshot
    {
        public PokemonSideSnapshot Player { get; set; } = new();
        public PokemonSideSnapshot Enemy { get; set; } = new();
        public IReadOnlyList<MoveSnapshot> PlayerMoves { get; set; } = Array.Empty<MoveSnapshot>();
        public IReadOnlyList<string> LogEntries { get; set; } = Array.Empty<string>();
        public bool IsOver { get; set; }
        public string? WinnerName { get; set; }
    }

    public class PokemonSideSnapshot
    {
        public int PokedexId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public string StatusCondition { get; set; } = string.Empty;
    }

    // ── Primitive representation of a move — no IMove dependency ─────────
    public class MoveSnapshot
    {
        public int Index { get; set; }   // 0-3, matches the slot
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? Power { get; set; }
        public int? Accuracy { get; set; }
        public int PP { get; set; }
    }
}
