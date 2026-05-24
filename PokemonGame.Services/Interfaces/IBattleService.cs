namespace PokemonGame.Services.Interfaces
{
    public interface IBattleService
    {
        event Action? OnStateUpdated;
        event Action<Exception>? OnError;

        bool IsConnected { get; }
        bool HasInitialState { get; }
        bool IsOver { get; }
        string? WinnerName { get; }

        Task ConnectAsync();
        Task WaitForInitialStateAsync(int timeoutMs = 10000);
        Task RunTurnAsync(int index, string action = "Move");
        Task ForfeitAsync();
        Task DisconnectAsync();
        Task RunMoveAsync(int moveIndex);
        Task RunSwitchAsync(int slotIndex);

        BattleSnapshot GetState();
    }
    public class BattleSnapshot
    {
        public PokemonSideSnapshot Player { get; set; } = new();
        public PokemonSideSnapshot Enemy { get; set; } = new();

        public List<MoveSnapshot> PlayerMoves { get; set; } = new();

        /// <summary>Log lines produced since the last snapshot.</summary>
        public IReadOnlyList<string> LogEntries { get; set; } = Array.Empty<string>();

        public bool IsOver { get; set; }

        /// <summary>Human-readable winner name (for display only).</summary>
        public string? WinnerName { get; set; }

        /// <summary>
        /// FIX #4 / FIX #2: the winning player's numeric ID.
        /// BattleViewModel compares this to BattlePlayerID to decide
        /// "YOU WON" vs "YOU LOST" reliably, without relying on string equality.
        /// Null while the battle is in progress.
        /// </summary>
        public int? WinnerPlayerId { get; set; }
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

    public class MoveSnapshot
    {
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int PP { get; set; }
        public int MaxPP { get; set; }
        public int? Power { get; set; }
        public int? Accuracy { get; set; }
    }
}
