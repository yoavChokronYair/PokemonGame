using PokemonGame.Interface;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Helper;
using PokemonGame.Services.Enums.PokemonEnum;

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

    // ── Nature (affects stat multipliers) ────────────────────────────────────

    public enum Nature
    {
        Hardy,   // neutral
        Lonely, Brave, Adamant, Naughty,            // +Atk
        Bold, Relaxed, Impish, Lax,                 // +Def
        Timid, Hasty, Jolly, Naive,                 // +Spe
        Modest, Mild, Quiet, Rash,                  // +SpAtk
        Calm, Gentle, Sassy, Careful,               // +SpDef
        Bashful, Docile, Quirky, Serious             // neutral
    }

    public static class NatureModifier
    {
        // Returns (boostedStat, hinderedStat) — null means neutral
        public static (Stat? boosted, Stat? hindered) GetModifiers(Nature nature) => nature switch
        {
            Nature.Lonely => (Stat.Attack, Stat.Defense),
            Nature.Brave => (Stat.Attack, Stat.Speed),
            Nature.Adamant => (Stat.Attack, Stat.SpecialAttack),
            Nature.Naughty => (Stat.Attack, Stat.SpecialDefense),
            Nature.Bold => (Stat.Defense, Stat.Attack),
            Nature.Relaxed => (Stat.Defense, Stat.Speed),
            Nature.Impish => (Stat.Defense, Stat.SpecialAttack),
            Nature.Lax => (Stat.Defense, Stat.SpecialDefense),
            Nature.Timid => (Stat.Speed, Stat.Attack),
            Nature.Hasty => (Stat.Speed, Stat.Defense),
            Nature.Jolly => (Stat.Speed, Stat.SpecialAttack),
            Nature.Naive => (Stat.Speed, Stat.SpecialDefense),
            Nature.Modest => (Stat.SpecialAttack, Stat.Attack),
            Nature.Mild => (Stat.SpecialAttack, Stat.Defense),
            Nature.Quiet => (Stat.SpecialAttack, Stat.Speed),
            Nature.Rash => (Stat.SpecialAttack, Stat.SpecialDefense),
            Nature.Calm => (Stat.SpecialDefense, Stat.Attack),
            Nature.Gentle => (Stat.SpecialDefense, Stat.Defense),
            Nature.Sassy => (Stat.SpecialDefense, Stat.Speed),
            Nature.Careful => (Stat.SpecialDefense, Stat.SpecialAttack),
            _ => (null, null) // neutral natures
        };
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
        public Nature Nature { get; }

        // ── Stats ─────────────────────────────────────────────────────────────
        public BaseStats Base { get; }
        private readonly int[] ivs = new int[6]; // 0–31
        private readonly int[] evs = new int[6]; // 0–252

        // Computed final stats (calculated once on construction)
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

        // ── Last damage taken (for Bide, Counter, Mirror Coat) ───────────────
        public int LastDamageTaken { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────

        public PokemonDomain(
            string name,
            int pokedexNumber,
            PokemonType primaryType,
            PokemonType? secondaryType,
            int level,
            Nature nature,
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

            // Calculate final stats using Gen 3+ formulas
            MaxHP = CalcHP();
            BaseAttack = CalcStat(Base.Attack, Stat.Attack, 0);
            BaseDefense = CalcStat(Base.Defense, Stat.Defense, 1);
            BaseSpecialAttack = CalcStat(Base.SpecialAttack, Stat.SpecialAttack, 2);
            BaseSpecialDefense = CalcStat(Base.SpecialDefense, Stat.SpecialDefense, 3);
            BaseSpeed = CalcStat(Base.Speed, Stat.Speed, 4);

            CurrentHP = MaxHP;
        }

        // ── Stat Calculation (Gen 3+ formula) ────────────────────────────────

        private int CalcHP()
        {
            // HP = floor((2 * Base + IV + floor(EV/4)) * Level / 100) + Level + 10
            return (int)Math.Floor((2.0 * Base.HP + ivs[0] + Math.Floor(evs[0] / 4.0))
                   * Level / 100.0) + Level + 10;
        }

        private int CalcStat(int baseStat, Stat stat, int index)
        {
            // Stat = floor((floor((2 * Base + IV + floor(EV/4)) * Level / 100) + 5) * nature)
            double raw = Math.Floor((2.0 * baseStat + ivs[index] + Math.Floor(evs[index] / 4.0))
                         * Level / 100.0) + 5;

            var (boosted, hindered) = NatureModifier.GetModifiers(Nature);
            if (boosted == stat) raw *= 1.1;
            if (hindered == stat) raw *= 0.9;

            return (int)Math.Floor(raw);
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

            // Paralysis halves speed
            if (stat == Stat.Speed && Status == StatusCondition.Paralysis)
                baseStat /= 2;

            // Burn halves attack
            if (stat == Stat.Attack && Status == StatusCondition.Burn)
                baseStat /= 2;

            double multiplier = stage switch
            {
                -6 => 2.0 / 8,
                -5 => 2.0 / 7,
                -4 => 2.0 / 6,
                -3 => 2.0 / 5,
                -2 => 2.0 / 4,
                -1 => 2.0 / 3,
                0 => 1.0,
                +1 => 3.0 / 2,
                +2 => 4.0 / 2,
                +3 => 5.0 / 2,
                +4 => 6.0 / 2,
                +5 => 7.0 / 2,
                +6 => 8.0 / 2,
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

        public void RestoreHP(int amount)
            => CurrentHP = Math.Min(MaxHP, CurrentHP + amount);

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

        public void ApplyStatus(StatusCondition status, int turns = 0)
        {
            if (!CanApplyStatus(status)) return;
            Status = status;
            ToxicCounter = 0;
            if (status == StatusCondition.Sleep)
                SleepTurns = turns > 0 ? turns : new Random().Next(1, 4);
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

        public bool HasVolatileStatus(VolatileStatus status)
            => volatileStatuses.ContainsKey(status);

        public void RemoveVolatileStatus(VolatileStatus status)
            => volatileStatuses.Remove(status);

        // ── Type Helpers ──────────────────────────────────────────────────────

        public bool HasType(PokemonType type)
            => PrimaryType == type || SecondaryType == type;

        // ── Move Helpers ──────────────────────────────────────────────────────

        public void CopyMove(IMove? move)
            => LastUsedMove = move;

        // ── Charge / Rampage ──────────────────────────────────────────────────

        public void BeginCharge(IAttempt release)
        {
            IsCharging = true;
            ChargeRelease = release;
        }

        public void EndCharge()
        {
            IsCharging = false;
            ChargeRelease = null;
        }

        public void BeginRampage(int turns)
        {
            IsRampaging = true;
            RampageTurnsLeft = turns;
        }

        public void DecrementRampage()
        {
            if (--RampageTurnsLeft <= 0)
                IsRampaging = false;
        }

        // ── Force Switch ──────────────────────────────────────────────────────

        public void ForceSwitch(BattleDomain battle)
        {
            ResetStatStages();
            volatileStatuses.Clear();
            IsCharging = false;
            IsRampaging = false;
            // Actual party swap is handled by BattleDomain / trainer logic
            battle.Log($"{Name} was forced out!");
        }

        public override string ToString() => $"{Name} (Lv.{Level}) {CurrentHP}/{MaxHP} HP [{Status}]";
    }
}