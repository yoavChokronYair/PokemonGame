using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model.Helper.PokemonHelper
{
    public class PokemonState
    {
        private readonly PokemonDomain _state;

        public PokemonState(PokemonDomain state)
        {
            _state = state;
        }

        // --- Identity / stats ---
        public int PokedexId => _state.PokedexNumber;
        public string Name => _state.Name;
        public int Level => _state.Level;
        public int MaxHP => _state.MaxHP;
        public int CurrentHP => _state.CurrentHP;
        public bool IsFainted => _state.CurrentHP <= 0;
        public int Attack => _state.BaseAttack;
        public int Defense => _state.BaseDefense;
        public int SpAttack => _state.BaseSpecialAttack;
        public int SpDefense => _state.BaseSpecialDefense;
        public int Speed => _state.BaseSpeed;
        public IReadOnlyList<IMove> Moves => _state.Moves;
        public IMove? LastUsedMove => _state.LastUsedMove;

        public double LastDamageDealt { get; internal set; }

        // --- HP ---
        public void TakeDamage(int amount)
        {
            _state.LastDamageTaken = Math.Min(amount, _state.CurrentHP);
            _state.CurrentHP = Math.Max(0, _state.CurrentHP - amount);
        }

        public void RestoreHP(int amount)
        {
            _state.CurrentHP = Math.Min(MaxHP, _state.CurrentHP + amount);
        }

        public void RegisterDamageDealt(int amount) => _state.LastDamageDealt = amount;
        public double GetHPFraction() => (double)CurrentHP / MaxHP;


        // --- Status ---
        public bool CanApplyStatus(StatusCondition newStatus)
        {
            if (_state.Status != StatusCondition.None)
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
        public StatusCondition PokemonStatusCondition() => _state.Status;
        public void ApplyStatus(StatusCondition status, int turns = 0)
        {
            if (!CanApplyStatus(status))
            {
                return;
            }

            _state.Status = status;
            _state.ToxicCounter = 0;
            if (status == StatusCondition.Sleep)
            {
                _state.SleepTurns = turns > 0 ? turns : RandomHelper.Next(1, 4);
            }
        }

        public void ClearStatus()
        {
            _state.Status = StatusCondition.None;
            _state.ToxicCounter = 0;
            _state.SleepTurns = 0;
        }
        public void ApplyToxicByOne()
        {
            this._state.ToxicCounter++;
        }
        public int getToxicCounter()
        {
            return this._state.ToxicCounter;
        }
        // --- Volatile Status ---
        public void ApplyVolatileStatus(VolatileStatus status, int turns = 0)
        {
            if (!_state.VolatileStatuses.ContainsKey(status))
            {
                _state.VolatileStatuses[status] = turns;
            }
        }

        public bool HasVolatileStatus(VolatileStatus status) => _state.VolatileStatuses.ContainsKey(status);
        public void RemoveVolatileStatus(VolatileStatus status) => _state.VolatileStatuses.Remove(status);

        // --- Type helpers ---
        public bool HasType(PokemonType type) => _state.PrimaryType == type || _state.SecondaryType == type;
        public PokemonType[] GetPokemonTypes() =>
            new[] { _state.PrimaryType, _state.SecondaryType ?? PokemonType.None }
            .Where(t => t != PokemonType.None)
            .ToArray();
        // --- Moves ---
        public void CopyMove(IMove? move) => _state.LastUsedMove = move;

        // --- Charge / Rampage ---
        public void BeginCharge(IAttempt release) { _state.IsCharging = true; _state.ChargeRelease = release; }
        public void EndCharge() { _state.IsCharging = false; _state.ChargeRelease = null; }
        public bool IsCharging() => _state.IsCharging;
        public void BeginRampage(int turns) { _state.IsRampaging = true; _state.RampageTurnsLeft = turns; }
        public void DecrementRampage()
        {
            if (--_state.RampageTurnsLeft <= 0)
            {
                _state.IsRampaging = false;
            }
        }
        public bool IsRampaging() => _state.IsRampaging;

        // --- Bide ---
        public void StartBide(int turns) { _state.IsBiding = true; _state.BideTurnsLeft = turns; _state.BideStoredDamage = 0; }
        public void AccumulateBideDamage(int damage) => _state.BideStoredDamage += damage;
        public void DecrementBide()
        {
            if (--_state.BideTurnsLeft <= 0)
            {
                _state.IsBiding = false;
            }
        }

        // --- Stat stages ---
        public int GetEffectiveStat(Stat stat)
        {
            int baseStat = stat switch
            {
                Stat.Attack => _state.BaseAttack,
                Stat.Defense => _state.BaseDefense,
                Stat.SpecialAttack => _state.BaseSpecialAttack,
                Stat.SpecialDefense => _state.BaseSpecialDefense,
                Stat.Speed => _state.BaseSpeed,
                _ => 1
            };

            int stage = _state.StatStages[stat];

            // Apply status modifiers
            if (stat == Stat.Speed && _state.Status == StatusCondition.Paralysis)
            {
                baseStat /= 2;
            }

            if (stat == Stat.Attack && _state.Status == StatusCondition.Burn)
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
            => _state.StatStages[stat] = MathHelper.Clamp(_state.StatStages[stat] + stages, -6, 6);

        public void ResetStatStages()
        {
            foreach (var key in _state.StatStages.Keys.ToList())
            {
                _state.StatStages[key] = 0;
            }
        }

        // --- Force switch / reset ---
        public void ForceSwitch(BattleState battle)
        {
            ResetStatStages();
            _state.VolatileStatuses.Clear();
            _state.IsCharging = false;
            _state.IsRampaging = false;
            battle.Logger.Log($"{Name} was forced out!");
        }

        public override string ToString() => $"{Name} (Lv.{Level}) {CurrentHP}/{MaxHP} HP [{_state.Status}]";
    }
}
