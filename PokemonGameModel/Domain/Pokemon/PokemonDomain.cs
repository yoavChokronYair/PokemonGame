// Design: Entity — represents one Pokemon in an active battle.
// Holds: identity, computed final stats, stat stages, HP, status, moves, charge/rampage state.
// OOP: Encapsulation — all HP/status/stage mutation is through public methods.
// Note: Nature enum was removed — use NatureType from Enums/NatureType.cs.
// Note: NatureModifier class was removed — use NatureHelper.GetNatureModifiers (DataHelper).
// Note: CalcHP/CalcStat delegate to PokemonStatCalculatorHelper (MathHelper) — no duplication.
// Note: ApplyStatus uses RandomHelper for sleep duration — no inline new Random().
// Note: All battle enums (Stat, StatusCondition, VolatileStatus) live in Enums/Battle/BattleEnums.cs.

using PokemonGame.Enums;
using PokemonGame.Enums.Battle;
using PokemonGame.Interface.Move;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Helper.DataHelper;
using PokemonGame.Core.Model.Helper.MathHelper;
using PokemonGame.Services.Enums.PokemonEnum;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Interface.Move;

namespace PokemonGame.Model.Domain.Pokemon
{
    // ── Base Stats (immutable — the species template) ─────────────────────────

    public class BaseStats
    {
        public int HP { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int SpecialAttack { get; }
        public int SpecialDefense { get; }
        public int Speed { get; }

        public BaseStats(int hp, int attack, int defense,
                         int specialAttack, int specialDefense, int speed)
        {
            HP = hp;
            Attack = attack;
            Defense = defense;
            SpecialAttack = specialAttack;
            SpecialDefense = specialDefense;
            Speed = speed;
        }
    }

    // ── Pokemon Domain ────────────────────────────────────────────────────────

    internal class PokemonDomain
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public string Name { get; }
        public int PokedexNumber { get; }
        public PokemonType PrimaryType { get; }
        public PokemonType? SecondaryType { get; }
        public int Level { get; }
        public NatureType Nature { get; }

        // ── Stats ─────────────────────────────────────────────────────────────
        public BaseStats Base { get; }
        private readonly int[] ivs = new int[6];
        private readonly int[] evs = new int[6];

        // Computed final stats (delegated to PokemonStatCalculatorHelper)
        public int MaxHP { get; }
        public int BaseAttack { get; }
        public int BaseDefense { get; }
        public int BaseSpecialAttack { get; }
        public int BaseSpecialDefense { get; }
        public int BaseSpeed { get; }

        // ── Stat Stages (battle-only, -6 to +6) ──────────────────────────────
        private readonly Dictionary<Stat, int> statStages = new()
        {
            { Stat.Attack, 0 }, { Stat.Defense, 0 },
            { Stat.SpecialAttack, 0 }, { Stat.SpecialDefense, 0 },
            { Stat.Speed, 0 }, { Stat.Accuracy, 0 }, { Stat.Evasion, 0 }
        };

        // ── HP ────────────────────────────────────────────────────────────────
        public int CurrentHP { get; private set; }
        public bool IsFainted => CurrentHP <= 0;
        public double HPFraction => (double)CurrentHP / MaxHP;

        // ── Status ────────────────────────────────────────────────────────────
        public StatusCondition Status { get; private set; } = StatusCondition.None;
        public int ToxicCounter { get; set; } = 0;
        public int SleepTurns { get; private set; } = 0;

        private readonly Dictionary<VolatileStatus, int> volatileStatuses = new();

        // ── Moves ─────────────────────────────────────────────────────────────
        public IReadOnlyList<IMove> Moves { get; }
        public IMove? LastUsedMove { get; private set; }

        // ── Charge / Rampage state ────────────────────────────────────────────
        public bool IsCharging { get; private set; }
        public IAttempt? ChargeRelease { get; private set; }
        public bool IsRampaging { get; private set; }
        public int RampageTurnsLeft { get; private set; }

        // ── Last damage (for Bide, Counter, Mirror Coat) ─────────────────────
        public int LastDamageTaken { get; private set; }
        public int LastDamageDealt { get; private set; }

        // ── Bide State ────────────────────────────────────────────────────────
        public bool IsBiding { get; private set; }
        public int BideTurnsLeft { get; private set; }
        public int BideStoredDamage { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────

        public PokemonDomain(
            string name,
            int pokedexNumber,
            PokemonType primaryType,
            PokemonType? secondaryType,
            int level,
            NatureType nature,
            BaseStats baseStats,
            List<IMove> moves,
            int[]? ivs = null,
            int[]? evs = null)
        {
            Name = name;
            PokedexNumber = pokedexNumber;
            PrimaryType = primaryType;
            SecondaryType = secondaryType;
            Level = level;
            Nature = nature;
            Base = baseStats;
            Moves = moves;

            this.ivs = ivs ?? new int[] { 31, 31, 31, 31, 31, 31 };
            this.evs = evs ?? new int[] { 0, 0, 0, 0, 0, 0 };

            // Delegate to PokemonStatCalculatorHelper — no duplicate formula here.
            var natureModifiers = NatureHelper.GetNatureModifiers(nature);
            MaxHP = PokemonStatCalculatorHelper.CalculateHP(Base.HP, this.ivs[0], this.evs[0], level);
            BaseAttack = PokemonStatCalculatorHelper.CalculateStat(Base.Attack, this.ivs[1], this.evs[1], level, natureModifiers.atk);
            BaseDefense = PokemonStatCalculatorHelper.CalculateStat(Base.Defense, this.ivs[2], this.evs[2], level, natureModifiers.def);
            BaseSpecialAttack = PokemonStatCalculatorHelper.CalculateStat(Base.SpecialAttack, this.ivs[3], this.evs[3], level, natureModifiers.spAtk);
            BaseSpecialDefense = PokemonStatCalculatorHelper.CalculateStat(Base.SpecialDefense, this.ivs[4], this.evs[4], level, natureModifiers.spDef);
            BaseSpeed = PokemonStatCalculatorHelper.CalculateStat(Base.Speed, this.ivs[5], this.evs[5], level, natureModifiers.speed);

            CurrentHP = MaxHP;
        }

        // ── Effective Stat (applies battle stages) ────────────────────────────

        public int GetEffectiveStat(Stat stat)
        {
            int baseStat = stat switch
            {
                Stat.Attack => BaseAttack,
                Stat.Defense => BaseDefense,
                Stat.SpecialAttack => BaseSpecialAttack,
                Stat.SpecialDefense => BaseSpecialDefense,
                Stat.Speed => BaseSpeed,
                _ => 1
            };

            int stage = statStages[stat];

            if (stat == Stat.Speed && Status == StatusCondition.Paralysis) baseStat /= 2;
            if (stat == Stat.Attack && Status == StatusCondition.Burn) baseStat /= 2;

            double multiplier = stage switch
            {
                -6 => 2.0 / 8, -5 => 2.0 / 7, -4 => 2.0 / 6,
                -3 => 2.0 / 5, -2 => 2.0 / 4, -1 => 2.0 / 3,
                0 => 1.0,
                +1 => 3.0 / 2, +2 => 4.0 / 2, +3 => 5.0 / 2,
                +4 => 6.0 / 2, +5 => 7.0 / 2, +6 => 8.0 / 2,
                _ => 1.0
            };

            return (int)(baseStat * multiplier);
        }

        // ── Stat Stage Manipulation ───────────────────────────────────────────

        public void ChangeStatStage(Stat stat, int stages)
            => statStages[stat] = MathHelper.Clamp(statStages[stat] + stages, -6, 6);

        public void ResetStatStages()
        {
            foreach (var key in statStages.Keys.ToList())
                statStages[key] = 0;
        }

        // ── HP Manipulation ───────────────────────────────────────────────────

        public void TakeDamage(int amount)
        {
            LastDamageTaken = Math.Min(amount, CurrentHP);
            CurrentHP = Math.Max(0, CurrentHP - amount);
        }

        public void RestoreHP(int amount) => CurrentHP = Math.Min(MaxHP, CurrentHP + amount);

        public void RegisterDamageDealt(int amount) => LastDamageDealt = amount;

        // ── Status Manipulation ───────────────────────────────────────────────

        public bool CanApplyStatus(StatusCondition newStatus)
        {
            if (Status != StatusCondition.None) return false;
            if (newStatus == StatusCondition.Burn && HasType(PokemonType.Fire)) return false;
            if (newStatus == StatusCondition.Freeze && HasType(PokemonType.Ice)) return false;
            if (newStatus == StatusCondition.Poison && HasType(PokemonType.Poison)) return false;
            if (newStatus == StatusCondition.Poison && HasType(PokemonType.Steel)) return false;
            return true;
        }

        // Sleep duration uses RandomHelper — no inline new Random().
        public void ApplyStatus(StatusCondition status, int turns = 0)
        {
            if (!CanApplyStatus(status)) return;
            Status = status;
            ToxicCounter = 0;
            if (status == StatusCondition.Sleep)
                SleepTurns = turns > 0 ? turns : RandomHelper.Next(1, 4);
        }

        public void ClearStatus()
        {
            Status = StatusCondition.None;
            ToxicCounter = 0;
            SleepTurns = 0;
        }

        // ── Volatile Status ───────────────────────────────────────────────────

        public void ApplyVolatileStatus(VolatileStatus status, int turns = 0)
        {
            if (!volatileStatuses.ContainsKey(status))
                volatileStatuses[status] = turns;
        }

        public bool HasVolatileStatus(VolatileStatus status) => volatileStatuses.ContainsKey(status);
        public void RemoveVolatileStatus(VolatileStatus status) => volatileStatuses.Remove(status);

        // ── Type Helpers ──────────────────────────────────────────────────────

        public bool HasType(PokemonType type) => PrimaryType == type || SecondaryType == type;

        // ── Move Helpers ──────────────────────────────────────────────────────

        public void CopyMove(IMove? move) => LastUsedMove = move;

        // ── Charge / Rampage ──────────────────────────────────────────────────

        public void BeginCharge(IAttempt release) { IsCharging = true; ChargeRelease = release; }
        public void EndCharge() { IsCharging = false; ChargeRelease = null; }
        public void BeginRampage(int turns) { IsRampaging = true; RampageTurnsLeft = turns; }
        public void DecrementRampage()
        {
            if (--RampageTurnsLeft <= 0) IsRampaging = false;
        }

        // ── Bide ──────────────────────────────────────────────────────────────

        public void StartBide(int turns) { IsBiding = true; BideTurnsLeft = turns; BideStoredDamage = 0; }
        public void AccumulateBideDamage(int damage) => BideStoredDamage += damage;
        public void DecrementBide()
        {
            if (--BideTurnsLeft <= 0) IsBiding = false;
        }

        // ── Force Switch ──────────────────────────────────────────────────────

        public void ForceSwitch(BattleState battle)
        {
            ResetStatStages();
            volatileStatuses.Clear();
            IsCharging = false;
            IsRampaging = false;
            battle.Logger.Log($"{Name} was forced out!");
        }

        public override string ToString() => $"{Name} (Lv.{Level}) {CurrentHP}/{MaxHP} HP [{Status}]";
    }
}
