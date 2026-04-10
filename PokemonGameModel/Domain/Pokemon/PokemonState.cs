using PokemonGame.Core.Config;
using PokemonGame.Enums;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Pokemon
{
    // Design: Entity — represents one Pokemon in an active battle.
    // Holds: identity, computed final stats, stat stages, HP, status, moves, charge/rampage state.
    // OOP: Encapsulation — all HP/status/stage mutation is through public methods.
    // Note: Nature enum was removed — use NatureType from Enums/NatureType.cs.
    // Note: NatureModifier class was removed — use NatureHelper.GetNatureModifiers (DataHelper).
    // Note: CalcHP/CalcStat delegate to PokemonStatCalculatorHelper (MathHelper) — no duplication.
    // Note: ApplyStatus uses RandomHelper for sleep duration — no inline new Random().
    // Note: All battle enums (Stat, StatusCondition, VolatileStatus) live in Enums/Battle/BattleEnums.cs.
    public class PokemonState
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public string Name { get; set; } = string.Empty;
        public int PokedexId { get; set; }
        public PokemonType PrimaryType { get; set; }
        public PokemonType? SecondaryType { get; set; }
        public IAbility? Ability { get; set; }
        public IHeldItem? HeldItem { get; set; }
        public int Level { get; set; }
        public NatureType Nature { get; set; }

        // ── Base Stats ────────────────────────────────────────────────────────
        public BaseStats Base { get; set; } = new();
        public int MaxHP { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseSpecialAttack { get; set; }
        public int BaseSpecialDefense { get; set; }
        public int BaseSpeed { get; set; }

        // ── IVs / EVs ─────────────────────────────────────────────────────────
        public int[] IVs { get; set; } = new int[PokemonConstants.IvsAndEvsNum];
        public int[] EVs { get; set; } = new int[PokemonConstants.IvsAndEvsNum];

        // ── HP / Faint ────────────────────────────────────────────────────────
        public int CurrentHP { get; private set; }
        public bool IsFainted => CurrentHP <= 0;
        public double GetHPFraction() => (double)CurrentHP / MaxHP;

        // ── Status ────────────────────────────────────────────────────────────
        public StatusCondition Status { get; private set; } = StatusCondition.None;
        public int ToxicCounter { get; private set; }
        public int SleepTurns { get; private set; }
        public Dictionary<VolatileStatus, int> VolatileStatuses { get; } = new();

        // ── Stat Stages ───────────────────────────────────────────────────────
        public Dictionary<Stat, int> StatStages { get; } = new()
        {
            { Stat.Attack, 0 }, { Stat.Defense, 0 },
            { Stat.SpecialAttack, 0 }, { Stat.SpecialDefense, 0 },
            { Stat.Speed, 0 }, { Stat.Accuracy, 0 }, { Stat.Evasion, 0 }
        };
        public bool WasStatLoweredThisTurn { get; set; }

        // ── Multipliers ───────────────────────────────────────────────────────
        public int CritStage { get; private set; }
        public double SpeedMultiplier { get; private set; } = 1.0;
        public double AccuracyMultiplier { get; private set; } = 1.0;
        public double EvasionMultiplier { get; private set; } = 1.0;

        // ── Moves ─────────────────────────────────────────────────────────────
        public List<IMove> Moves { get; set; } = new();
        public IMove? LastUsedMove { get; private set; }
        public IMove? LockedMove { get; private set; }
        public int? PriorityOverride { get; private set; }

        // ── Charge / Rampage / Bide ───────────────────────────────────────────
        public bool IsChargingState { get; private set; }
        public IAttempt? ChargeRelease { get; private set; }
        public bool IsRampagingState { get; private set; }
        public int RampageTurnsLeft { get; private set; }
        public bool IsBidingState { get; private set; }
        public int BideTurnsLeft { get; private set; }
        public int BideStoredDamage { get; private set; }

        // ── Damage Tracking ───────────────────────────────────────────────────
        public double LastDamageDealt { get; internal set; }
        public double LastDamageTaken { get; internal set; }
        public int turnsActive;

        public PokemonState() { }

        // ── HP ────────────────────────────────────────────────────────────────
        public void TakeDamage(int amount)
        {
            LastDamageTaken = Math.Min(amount, CurrentHP);
            CurrentHP = Math.Max(0, CurrentHP - amount);
        }

        public void RestoreHP(int amount) => CurrentHP = Math.Min(MaxHP, CurrentHP + amount);
        public void RegisterDamageDealt(int amount) => LastDamageDealt = amount;
        public void RegisterDamageTaken(int amount) => LastDamageTaken = amount;

        // ── Status ────────────────────────────────────────────────────────────
        public bool CanApplyStatus(StatusCondition newStatus)
        {
            if (Status != StatusCondition.None) return false;

            AbilityState ability = (AbilityState)Ability;
            switch (newStatus)
            {
                case StatusCondition.Burn:
                    if (HasType(PokemonType.Fire)) return false;
                    if (ability.Name == "Water Veil") return false;
                    break;
                case StatusCondition.Freeze:
                    if (HasType(PokemonType.Ice)) return false;
                    if (ability.Name == "Magma Armor") return false;
                    break;
                case StatusCondition.Paralysis:
                    if (HasType(PokemonType.Electric)) return false;
                    if (ability.Name == "Limber") return false;
                    break;
                case StatusCondition.Poison:
                    if (HasType(PokemonType.Poison) || HasType(PokemonType.Steel)) return false;
                    if (ability.Name == "Immunity") return false;
                    break;
                case StatusCondition.Sleep:
                    if (ability.Name == "Insomnia" || ability.Name == "Vital Spirit") return false;
                    break;
            }

            return true;
        }

        public void ApplyStatus(StatusCondition status, int turns = 0)
        {
            if (!CanApplyStatus(status)) return;
            Status = status;
            ToxicCounter = 0;
            if (status == StatusCondition.Sleep)
                SleepTurns = turns > 0 ? turns : RandomHelper.Next(1, 4);
        }

        public void ClearStatus() { Status = StatusCondition.None; ToxicCounter = 0; SleepTurns = 0; }
        public void ApplyToxicByOne() => ToxicCounter++;
        public int GetToxicCounter() => ToxicCounter;
        public StatusCondition PokemonStatusCondition() => Status;

        // ── Volatile Status ───────────────────────────────────────────────────
        public void ApplyVolatileStatus(VolatileStatus status, int turns = 0)
        {
            if (!VolatileStatuses.ContainsKey(status))
                VolatileStatuses[status] = turns;
        }

        public bool HasVolatileStatus(VolatileStatus status) => VolatileStatuses.ContainsKey(status);
        public void RemoveVolatileStatus(VolatileStatus status) => VolatileStatuses.Remove(status);

        // ── Type Helpers ──────────────────────────────────────────────────────
        public bool HasType(PokemonType type) => PrimaryType == type || SecondaryType == type;
        public PokemonType[] GetPokemonTypes() =>
            new[] { PrimaryType, SecondaryType ?? PokemonType.None }
            .Where(t => t != PokemonType.None)
            .ToArray();

        // ── Moves ─────────────────────────────────────────────────────────────
        public void CopyMove(IMove? move) => LastUsedMove = move;
        public void LockToLastMove() => LockedMove = LastUsedMove;
        public IMove? GetLockedMove() => LockedMove;
        public void SetPriorityOverride(int priority) => PriorityOverride = priority;
        public int? GetPriorityOverride() => PriorityOverride;
        public void ClearPriorityOverride() => PriorityOverride = null;

        // ── Charge / Rampage / Bide ───────────────────────────────────────────
        public void BeginCharge(IAttempt release) { IsChargingState = true; ChargeRelease = release; }
        public void EndCharge() { IsChargingState = false; ChargeRelease = null; }
        public bool IsCharging() => IsChargingState;
        public void BeginRampage(int turns) { IsRampagingState = true; RampageTurnsLeft = turns; }
        public void DecrementRampage() { if (--RampageTurnsLeft <= 0) IsRampagingState = false; }
        public bool IsRampaging() => IsRampagingState;
        public void StartBide(int turns) { IsBidingState = true; BideTurnsLeft = turns; BideStoredDamage = 0; }
        public void AccumulateBideDamage(int damage) => BideStoredDamage += damage;
        public void DecrementBide() { if (--BideTurnsLeft <= 0) IsBidingState = false; }

        // ── Stat Stages ───────────────────────────────────────────────────────
        public int GetEffectiveStat(Stat stat)
        {
            int baseStat = stat switch
            {
                Stat.Attack => BaseAttack,
                Stat.Defense => BaseDefense,
                Stat.SpecialAttack => BaseSpecialAttack,
                Stat.SpecialDefense => BaseSpecialDefense,
                Stat.Speed => (int)(BaseSpeed * SpeedMultiplier),
                _ => 1
            };

            if (stat == Stat.Speed && Status == StatusCondition.Paralysis) baseStat /= 2;
            if (stat == Stat.Attack && Status == StatusCondition.Burn) baseStat /= 2;

            int stage = StatStages[stat];
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

        public void ChangeStatStage(Stat stat, int stages)
            => StatStages[stat] = MathHelper.Clamp(StatStages[stat] + stages, -6, 6);

        public bool CanIncreaseStat(Stat stat)
            => StatStages.TryGetValue(stat, out int stage) && stage < 6;

        public void ResetStatStages()
        {
            foreach (var key in StatStages.Keys.ToList()) StatStages[key] = 0;
        }

        public void ResetNegativeStatStages()
        {
            foreach (var key in StatStages.Keys.ToList())
                if (StatStages[key] < 0) StatStages[key] = 0;
        }

        // ── Crit ──────────────────────────────────────────────────────────────
        public void RaiseCritStage(int stages) => CritStage = MathHelper.Clamp(CritStage + stages, 0, 3);
        public int GetCritStage() => CritStage;

        // ── Multipliers ───────────────────────────────────────────────────────
        public void ApplySpeedMultiplier(double multiplier) => SpeedMultiplier *= multiplier;
        public void ApplyAccuracyMultiplier(double multiplier) => AccuracyMultiplier *= multiplier;
        public void ApplyEvasionMultiplier(double multiplier) => EvasionMultiplier *= multiplier;
        public double GetSpeedMultiplier() => SpeedMultiplier;
        public double GetAccuracyMultiplier() => AccuracyMultiplier;
        public double GetEvasionMultiplier() => EvasionMultiplier;

        // ── Force Switch ──────────────────────────────────────────────────────
        public void ForceSwitch(BattleState battle)
        {
            ResetStatStages();
            VolatileStatuses.Clear();
            IsChargingState = false;
            IsRampagingState = false;
            CritStage = 0;
            SpeedMultiplier = 1.0;
            AccuracyMultiplier = 1.0;
            EvasionMultiplier = 1.0;
            LockedMove = null;
            PriorityOverride = null;
            battle.Logger.Log($"{Name} was forced out!");
        }

        public override string ToString() => $"{Name} (Lv.{Level}) {CurrentHP}/{MaxHP} HP [{Status}]";
    }
}