namespace PokemonGame.Services.Data.GameData.Move
{
    public sealed class MoveData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Element { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Target { get; set; } = "Opponent";
        public int PP { get; set; }
        public int Priority { get; set; }       // move priority bracket (e.g. +1 = Quick Attack, -6 = Trick Room)
        public int CritStage { get; set; }      // bonus crit stages (0 = normal, 1 = high-crit moves)
        public string Description { get; set; } = string.Empty;

        // FK children
        public List<AttemptRow> Attempts { get; set; } = new();
    }

    public sealed class MoveNumberData
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public double? ExactValue { get; set; }
        public double? RangeMin { get; set; }
        public double? RangeMax { get; set; }
        public int? LeftNumberId { get; set; }
        public int? RightNumberId { get; set; }
        public string? Target { get; set; }

        // FK children
        public List<MoveWeightedEntryData> WeightedEntries { get; set; } = new();  // Weighted type only
        public MoveNumberData? LeftNumber { get; set; }                             // Product / Sum / Quotient
        public MoveNumberData? RightNumber { get; set; }                            // Product / Sum / Quotient
    }

    public class MoveWeightedEntryData
    {
        public int Id { get; set; }
        public int NumberId { get; set; }
        public double Value { get; set; }
        public double Weight { get; set; }
    }

    public class ConditionRow
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public double? Probability { get; set; }
        public string? Weather { get; set; }
        public string? Status { get; set; }
        public string? VolatileStatus { get; set; }
        public double? HpFraction { get; set; }
        public string? PokemonType { get; set; }
        public int? LeftConditionId { get; set; }
        public int? RightConditionId { get; set; }
        public int? InnerConditionId { get; set; }

        // FK children
        public ConditionRow? LeftCondition { get; set; }   // And / Or
        public ConditionRow? RightCondition { get; set; }  // And / Or
        public ConditionRow? InnerCondition { get; set; }  // Not / UserCondition / OpponentCondition
    }

    public class EffectRow
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Target { get; set; }
        public int? NumberId { get; set; }
        public string? HealTarget { get; set; }
        public double? ChanceProbability { get; set; }
        public int? ChildEffectId { get; set; }
        public int? ConditionId { get; set; }
        public int? OnPassEffectId { get; set; }
        public int? OnFailEffectId { get; set; }
        public string? Stat { get; set; }
        public int? StatStages { get; set; }
        public int? SleepMinTurns { get; set; }
        public int? SleepMaxTurns { get; set; }
        public int? ConfuseMinTurns { get; set; }
        public int? ConfuseMaxTurns { get; set; }
        public int IsToxic { get; set; } = 0;
        public string? Weather { get; set; }
        public int? WeatherTurns { get; set; }
        public string? Screen { get; set; }
        public int? ScreenTurns { get; set; }
        public string? BattleSide { get; set; }
        public string? Hazard { get; set; }
        public int? ChargeTurns { get; set; }

        // FK children
        public MoveNumberData? Number { get; set; }              // damage / heal formula
        public EffectRow? ChildEffect { get; set; }              // Chance
        public ConditionRow? Condition { get; set; }             // Conditional
        public EffectRow? OnPassEffect { get; set; }             // Conditional
        public EffectRow? OnFailEffect { get; set; }             // Conditional
        public List<SequenceStepRow> SequenceSteps { get; set; } = new();       // Sequence
        public List<MultiStatChangeRow> MultiStatChanges { get; set; } = new(); // MultiStatChange
    }

    public class SequenceStepRow
    {
        public int SequenceEffectId { get; set; }
        public int StepOrder { get; set; }
        public int ChildEffectId { get; set; }

        // FK children
        public EffectRow? ChildEffect { get; set; }
    }

    public class MultiStatChangeRow
    {
        public int Id { get; set; }
        public int EffectId { get; set; }
        public string Stat { get; set; } = string.Empty;
        public int Stages { get; set; }
    }

    public class AttemptRow
    {
        public int Id { get; set; }
        public int MoveId { get; set; }
        public string Type { get; set; } = string.Empty;
        public double? AccuracyValue { get; set; }
        public int? OnHitEffectId { get; set; }
        public int? OnMissEffectId { get; set; }
        public int? AfterEffectId { get; set; }
        public int StopOnMiss { get; set; } = 1;
        public int? HitsNumberId { get; set; }
        public int? ChargeEffectId { get; set; }
        public int? ReleaseAttemptId { get; set; }
        public int? RampageMinTurns { get; set; }
        public int? RampageMaxTurns { get; set; }
        public int? AfterRampageEffectId { get; set; }

        // FK children
        public EffectRow? OnHitEffect { get; set; }
        public EffectRow? OnMissEffect { get; set; }
        public EffectRow? AfterEffect { get; set; }
        public MoveNumberData? HitsNumber { get; set; }          // Combo: how many hits
        public EffectRow? ChargeEffect { get; set; }             // Charge
        public AttemptRow? ReleaseAttempt { get; set; }          // Charge: self-ref
        public EffectRow? AfterRampageEffect { get; set; }       // Rampage
        public List<CascadeStepRow> CascadeSteps { get; set; } = new();  // Cascade
    }

    public class CascadeStepRow
    {
        public int CascadeAttemptId { get; set; }
        public int StepOrder { get; set; }
        public int ChildAttemptId { get; set; }

        // FK children
        public AttemptRow? ChildAttempt { get; set; }
    }
}