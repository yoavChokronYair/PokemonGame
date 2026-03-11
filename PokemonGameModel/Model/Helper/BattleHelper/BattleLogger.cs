// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

using PokemonGame.Enums.Battle;
using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class BattleLogger
    {
        private readonly List<string> battleLog = new();
        public IReadOnlyList<string> BattleLog => battleLog;
        public void Log(string message) => battleLog.Add(message);
    }

   
}