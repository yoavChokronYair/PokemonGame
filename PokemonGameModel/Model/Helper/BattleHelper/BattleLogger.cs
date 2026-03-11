// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class BattleLogger
    {
        private readonly List<string> _battleLog = new();
        public IReadOnlyList<string> BattleLog => _battleLog;
        public void Log(string message) => _battleLog.Add(message);
    }


}