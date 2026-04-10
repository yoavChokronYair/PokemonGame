// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.


// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.


// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.


// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Battle
{
    public class BattleLogEntry
    {
        public BattleLogPhase Phase { get; }
        public int Turn { get; }
        public string Message { get; }

        public BattleLogEntry(BattleLogPhase phase, int turn, string message)
        {
            Phase = phase;
            Turn = turn;
            Message = message;
        }
    }

    public class BattleLogger
    {
        private readonly List<BattleLogEntry> _entries = new();
        public int CurrentTurn { get; set; } = 0;

        public IReadOnlyList<BattleLogEntry> Entries => _entries;

        // Keeps backward compatibility — existing code calling BattleLog still works
        public IReadOnlyList<string> BattleLog =>
            _entries.Select(e => e.Message).ToList();

        public void Log(string message, BattleLogPhase phase = BattleLogPhase.Action)
            => _entries.Add(new BattleLogEntry(phase, CurrentTurn, message));

        // Convenience overloads so call sites are expressive
        public void LogSetup(string message) => Log(message, BattleLogPhase.Setup);
        public void LogTurnStart(string message) => Log(message, BattleLogPhase.TurnStart);
        public void LogFaint(string message) => Log(message, BattleLogPhase.Faint);
        public void LogSwitch(string message) => Log(message, BattleLogPhase.Switch);
        public void LogWeather(string message) => Log(message, BattleLogPhase.Weather);
        public void LogStatus(string message) => Log(message, BattleLogPhase.StatusEffect);
        public void LogBattleEnd(string message) => Log(message, BattleLogPhase.BattleEnd);
    }


}