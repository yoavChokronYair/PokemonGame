// Design: Entity — represents one Pokemon in an active battle.
// Holds: identity, computed final stats, stat stages, HP, status, moves, charge/rampage state.
// OOP: Encapsulation — all HP/status/stage mutation is through public methods.
// Note: Nature enum was removed — use NatureType from Enums/NatureType.cs.
// Note: NatureModifier class was removed — use NatureHelper.GetNatureModifiers (DataHelper).
// Note: CalcHP/CalcStat delegate to PokemonStatCalculatorHelper (MathHelper) — no duplication.
// Note: ApplyStatus uses RandomHelper for sleep duration — no inline new Random().
// Note: All battle enums (Stat, StatusCondition, VolatileStatus) live in Enums/Battle/BattleEnums.cs.

using PokemonGame.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Helper;

namespace PokemonGame.Model.Domain.Pokemon
{
    // ── Pokemon Domain ────────────────────────────────────────────────────────

    internal class PokemonDomain
    {
        public string Name { get; set; } = string.Empty;
        public int PokedexNumber { get; set; }
        public PokemonType PrimaryType { get; set; }
        public PokemonType? SecondaryType { get; set; }
        public int Level { get; set; }
        public NatureType Nature { get; set; }

        // Base stats
        public BaseStats Base { get; set; } = new BaseStats();
        public int MaxHP { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseSpecialAttack { get; set; }
        public int BaseSpecialDefense { get; set; }
        public int BaseSpeed { get; set; }

        // IVs / EVs
        public int[] IVs { get; set; } = new int[6];
        public int[] EVs { get; set; } = new int[6];

        // Stat stages
        public Dictionary<Stat, int> StatStages { get; set; } = new()
    {
        { Stat.Attack, 0 }, { Stat.Defense, 0 },
        { Stat.SpecialAttack, 0 }, { Stat.SpecialDefense, 0 },
        { Stat.Speed, 0 }, { Stat.Accuracy, 0 }, { Stat.Evasion, 0 }
    };

        // HP
        public int CurrentHP { get; set; }
        public StatusCondition Status { get; set; } = StatusCondition.None;
        public int ToxicCounter { get; set; } = 0;
        public int SleepTurns { get; set; } = 0;

        public Dictionary<VolatileStatus, int> VolatileStatuses { get; set; } = new();

        // Moves
        public List<IMove> Moves { get; set; } = new();
        public IMove? LastUsedMove { get; set; }

        // Charge / Rampage
        public bool IsCharging { get; set; }
        public IAttempt? ChargeRelease { get; set; }
        public bool IsRampaging { get; set; }
        public int RampageTurnsLeft { get; set; }

        // Bide
        public bool IsBiding { get; set; }
        public int BideTurnsLeft { get; set; }
        public int BideStoredDamage { get; set; }

        // Last damage
        public int LastDamageTaken { get; set; }
        public int LastDamageDealt { get; set; }
    }
}
