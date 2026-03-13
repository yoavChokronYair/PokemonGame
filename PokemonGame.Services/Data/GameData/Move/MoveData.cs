namespace PokemonGame.Services.Data.GameData.Move
{
    public sealed class MoveData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // PokemonType enum value
        public string Element { get; set; } = string.Empty;  // MoveCategory enum value
        public string Category { get; set; } = string.Empty;  // MoveTarget enum value
        public string Target { get; set; } = "Opponent";
        public int PP { get; set; }
    }
    public sealed class MoveNumberData
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public double? ExactValue { get; set; }   // Exactly
        public double? RangeMin { get; set; }   // Between
        public double? RangeMax { get; set; }   // Between
        public int? LeftNumberId { get; set; }   // Product / Sum / Quotient
        public int? RightNumberId { get; set; }   // Product / Sum / Quotient
        public string? Target { get; set; }   // MaxHP / CurrentHP / Level / LastDamageDealt
    }
    public class MoveWeightedEntryData
    {
        public int Id { get; set; }
        public int NumberId { get; set; }   // FK → numbers.id
        public double Value { get; set; }
        public double Weight { get; set; }
    }
    public class ConditionRow
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public double? Probability { get; set; }   // Probability
        public string? Weather { get; set; }   // IsWeatherActive
        public string? Status { get; set; }   // HasStatus
        public string? VolatileStatus { get; set; }   // HasVolatile
        public double? HpFraction { get; set; }   // HPBelow
        public string? PokemonType { get; set; }   // HasType
        public int? LeftConditionId { get; set; }   // And / Or
        public int? RightConditionId { get; set; }   // And / Or
        public int? InnerConditionId { get; set; }   // Not / UserCondition / OpponentCondition
    }
    public class EffectRow
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;

        // Shared ---------------------------------------------------
        public string? Target { get; set; }   // Attacker / Defender

        // Damage / Healing -----------------------------------------
        public int? NumberId { get; set; }   // FK → numbers.id
        public string? HealTarget { get; set; }   // Drain only

        // Chance ---------------------------------------------------
        public double? ChanceProbability { get; set; }
        public int? ChildEffectId { get; set; }   // FK → effects.id

        // Conditional ----------------------------------------------
        public int? ConditionId { get; set; }   // FK → conditions.id
        public int? OnPassEffectId { get; set; }   // FK → effects.id
        public int? OnFailEffectId { get; set; }   // FK → effects.id  (nullable)

        // StatChange -----------------------------------------------
        public string? Stat { get; set; }   // Stat enum value
        public int? StatStages { get; set; }

        // Sleep ----------------------------------------------------
        public int? SleepMinTurns { get; set; }
        public int? SleepMaxTurns { get; set; }

        // Confuse --------------------------------------------------
        public int? ConfuseMinTurns { get; set; }
        public int? ConfuseMaxTurns { get; set; }

        // Poison ---------------------------------------------------
        public int IsToxic { get; set; } = 0;  // 0 = Poison, 1 = Toxic

        // Field effects --------------------------------------------
        public string? Weather { get; set; }   // Weather enum value
        public int? WeatherTurns { get; set; }
        public string? Screen { get; set; }   // Screen enum value
        public int? ScreenTurns { get; set; }
        public string? BattleSide { get; set; }   // BattleSide enum value
        public string? Hazard { get; set; }   // Hazard enum value

        // StoreAndRelease (Bide) -----------------------------------
        public int? ChargeTurns { get; set; }
    }
    public class SequenceStepRow
    {
        public int SequenceEffectId { get; set; }   // FK → effects.id  (the Sequence parent)
        public int StepOrder { get; set; }   // 0-based position
        public int ChildEffectId { get; set; }   // FK → effects.id
    }
    public class MultiStatChangeRow
    {
        public int Id { get; set; }
        public int EffectId { get; set; }   // FK → effects.id
        public string Stat { get; set; } = string.Empty;  // Stat enum value
        public int Stages { get; set; }
    }
    public class AttemptRow
    {
        public int Id { get; set; }
        public int MoveId { get; set; }   // FK → moves.id
        public string Type { get; set; } = string.Empty;

        // Attempt + Combo ------------------------------------------
        public double? AccuracyValue { get; set; }

        // Attempt --------------------------------------------------
        public int? OnHitEffectId { get; set; }   // FK → effects.id
        public int? OnMissEffectId { get; set; }   // FK → effects.id
        public int? AfterEffectId { get; set; }   // FK → effects.id

        // Cascade --------------------------------------------------
        public int StopOnMiss { get; set; } = 1;  // 1 = true

        // Combo ----------------------------------------------------
        public int? HitsNumberId { get; set; }   // FK → numbers.id

        // Charge ---------------------------------------------------
        public int? ChargeEffectId { get; set; }   // FK → effects.id
        public int? ReleaseAttemptId { get; set; }   // FK → attempts.id (self-ref)

        // Rampage --------------------------------------------------
        public int? RampageMinTurns { get; set; }
        public int? RampageMaxTurns { get; set; }
        public int? AfterRampageEffectId { get; set; }   // FK → effects.id
    }

    /// <summary>
    /// Maps to the <c>cascade_steps</c> table.
    /// Each row is one ordered sub-attempt inside a <c>Cascade</c> attempt.
    /// </summary>
    public class CascadeStepRow
    {
        public int CascadeAttemptId { get; set; }   // FK → attempts.id  (the Cascade parent)
        public int StepOrder { get; set; }   // 0-based position
        public int ChildAttemptId { get; set; }   // FK → attempts.id
    }
}