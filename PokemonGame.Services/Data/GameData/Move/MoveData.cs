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
        public int Priority { get; set; }
        public int CritStage { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public sealed class NumberData
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
        public NumberData? LeftNumber { get; set; }                             // Product / Sum / Quotient
        public NumberData? RightNumber { get; set; }                            // Product / Sum / Quotient
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

    public class AttemptRow
    {
        public int Id { get; set; }
        public int MoveId { get; set; }
        public string Type { get; set; } = string.Empty;
        public double? AccuracyValue { get; set; }
        public int? OnHitEffectId { get; set; }
        public int? OnMissEffectId { get; set; }
        public int? AfterEffectId { get; set; }
        public int StopOnMiss { get; set; }
        public int? HitsNumberId { get; set; }
        public int? ChargeEffectId { get; set; }
        public int? ReleaseAttemptId { get; set; }
        public int? RampageMinTurns { get; set; }
        public int? RampageMaxTurns { get; set; }
        public int? AfterRampageEffectId { get; set; }
        public List<CascadeStepRow> CascadeSteps { get; set; } = new();
       
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
        public int IsToxic { get; set; }
        public string? Weather { get; set; }
        public int? WeatherTurns { get; set; }
        public string? Screen { get; set; }
        public int? ScreenTurns { get; set; }
        public string? BattleSide { get; set; }
        public string? Hazard { get; set; }
        public int? ChargeTurns { get; set; }
        public double? Multiplier { get; set; }
        public double? Fraction { get; set; }
        public string? VolatileStatus { get; set; }
        public string? Status { get; set; }
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



    public class CascadeStepRow
    {
        public int CascadeAttemptId { get; set; }
        public int StepOrder { get; set; }
        public int ChildAttemptId { get; set; }

        // FK children
        public AttemptRow? ChildAttempt { get; set; }
    }
}