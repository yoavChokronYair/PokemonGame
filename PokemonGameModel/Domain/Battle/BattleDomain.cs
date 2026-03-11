// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Domain.Battle
{


    // ── Battle Domain ─────────────────────────────────────────────────────────
    internal class BattleDomain
    {
        public PokemonState Attacker { get; set; }
        public PokemonState Defender { get; set; }

        public BattleSideState AttackerSide { get; } = new();
        public BattleSideState DefenderSide { get; } = new();

        public IMove? LastUsedMove { get; set; }
        public PokemonType? ActiveTypeOverride { get; set; } = null;
        public int TurnNumber { get; set; } = 0;
        public int LastDamageDealt { get; set; } = 0;

        // You can also optionally store immutable references to services if you want,
        // or keep them separate in a "BattleDomain" wrapper

    }
}
