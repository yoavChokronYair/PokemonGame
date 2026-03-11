using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model.Helper.PokemonHelper
{
    internal class PokemonState
    {
        private readonly PokemonDomain state;

        public PokemonState(PokemonDomain state)
        {
            this.state = state;
        }

        // --- Identity / stats ---
        public string Name => state.Name;
        public int Level => state.Level;
        public int MaxHP => state.MaxHP;
        public int CurrentHP => state.CurrentHP;
        public bool IsFainted => state.CurrentHP <= 0;

        public IReadOnlyList<IMove> Moves => state.Moves;
        public IMove? LastUsedMove => state.LastUsedMove;

        public double LastDamageDealt { get; internal set; }

        // --- HP ---
        public void TakeDamage(int amount)
        {
            state.LastDamageTaken = Math.Min(amount, state.CurrentHP);
            state.CurrentHP = Math.Max(0, state.CurrentHP - amount);
        }

        public void RestoreHP(int amount)
        {
            state.CurrentHP = Math.Min(MaxHP, state.CurrentHP + amount);
        }

        public void RegisterDamageDealt(int amount) => state.LastDamageDealt = amount;
        public double GetHPFraction() => (double)CurrentHP / MaxHP;


        // --- Status ---
        public bool CanApplyStatus(StatusCondition newStatus)
        {
            if (state.Status != StatusCondition.None)
            {
                return false;
            }

            if (newStatus == StatusCondition.Burn && HasType(PokemonType.Fire))
            {
                return false;
            }

            if (newStatus == StatusCondition.Freeze && HasType(PokemonType.Ice))
            {
                return false;
            }

            if (newStatus == StatusCondition.Poison && (HasType(PokemonType.Poison) || HasType(PokemonType.Steel)))
            {
                return false;
            }

            return true;
        }
        public StatusCondition PokemonStatusCondition() => state.Status;
        public void ApplyStatus(StatusCondition status, int turns = 0)
        {
            if (!CanApplyStatus(status))
            {
                return;
            }

            state.Status = status;
            state.ToxicCounter = 0;
            if (status == StatusCondition.Sleep)
            {
                state.SleepTurns = turns > 0 ? turns : RandomHelper.Next(1, 4);
            }
        }

        public void ClearStatus()
        {
            state.Status = StatusCondition.None;
            state.ToxicCounter = 0;
            state.SleepTurns = 0;
        }
        public void ApplyToxicByOne()
        {
            this.state.ToxicCounter++;
        }
        public int getToxicCounter()
        {
            return this.state.ToxicCounter;
        }
        // --- Volatile Status ---
        public void ApplyVolatileStatus(VolatileStatus status, int turns = 0)
        {
            if (!state.VolatileStatuses.ContainsKey(status))
            {
                state.VolatileStatuses[status] = turns;
            }
        }

        public bool HasVolatileStatus(VolatileStatus status) => state.VolatileStatuses.ContainsKey(status);
        public void RemoveVolatileStatus(VolatileStatus status) => state.VolatileStatuses.Remove(status);

        // --- Type helpers ---
        public bool HasType(PokemonType type) => state.PrimaryType == type || state.SecondaryType == type;

        // --- Moves ---
        public void CopyMove(IMove? move) => state.LastUsedMove = move;

        // --- Charge / Rampage ---
        public void BeginCharge(IAttempt release) { state.IsCharging = true; state.ChargeRelease = release; }
        public void EndCharge() { state.IsCharging = false; state.ChargeRelease = null; }
        public bool IsCharging() => state.IsCharging;
        public void BeginRampage(int turns) { state.IsRampaging = true; state.RampageTurnsLeft = turns; }
        public void DecrementRampage() { if (--state.RampageTurnsLeft <= 0)
            {
                state.IsRampaging = false;
            }
        }
        public bool IsRampaging() => state.IsRampaging;

        // --- Bide ---
        public void StartBide(int turns) { state.IsBiding = true; state.BideTurnsLeft = turns; state.BideStoredDamage = 0; }
        public void AccumulateBideDamage(int damage) => state.BideStoredDamage += damage;
        public void DecrementBide() { if (--state.BideTurnsLeft <= 0)
            {
                state.IsBiding = false;
            }
        }

        // --- Stat stages ---
        public int GetEffectiveStat(Stat stat)
        {
            int baseStat = stat switch
            {
                Stat.Attack => state.BaseAttack,
                Stat.Defense => state.BaseDefense,
                Stat.SpecialAttack => state.BaseSpecialAttack,
                Stat.SpecialDefense => state.BaseSpecialDefense,
                Stat.Speed => state.BaseSpeed,
                _ => 1
            };

            int stage = state.StatStages[stat];

            // Apply status modifiers
            if (stat == Stat.Speed && state.Status == StatusCondition.Paralysis)
            {
                baseStat /= 2;
            }

            if (stat == Stat.Attack && state.Status == StatusCondition.Burn)
            {
                baseStat /= 2;
            }

            // Stage multiplier
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
            => state.StatStages[stat] = MathHelper.Clamp(state.StatStages[stat] + stages, -6, 6);

        public void ResetStatStages()
        {
            foreach (var key in state.StatStages.Keys.ToList())
            {
                state.StatStages[key] = 0;
            }
        }

        // --- Force switch / reset ---
        public void ForceSwitch(BattleState battle)
        {
            ResetStatStages();
            state.VolatileStatuses.Clear();
            state.IsCharging = false;
            state.IsRampaging = false;
            battle.Logger.Log($"{Name} was forced out!");
        }

        public override string ToString() => $"{Name} (Lv.{Level}) {CurrentHP}/{MaxHP} HP [{state.Status}]";
    }
}
