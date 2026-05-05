namespace PokemonGame.Services.Data.GameData.Move
{
    // ── Assembled Number ─────────────────────────────────────────────────────────
    // Fully resolved - children already hydrated, no dangling IDs
    public class MoveNumber
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;

        // Exactly
        public double? ExactValue { get; set; }

        // Between
        public double? RangeMin { get; set; }
        public double? RangeMax { get; set; }

        // Weighted
        public List<MoveWeightedEntryData> WeightedEntries { get; set; } = new();

        // Product / Sum / Quotient
        public MoveNumber? Left { get; set; }
        public MoveNumber? Right { get; set; }

        // MaxHP / CurrentHP / Level / LastDamageDealt
        public string? Target { get; set; }
    }

    // ── Assembled Condition ──────────────────────────────────────────────────────
    public class MoveCondition
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;

        public double? Probability { get; set; }
        public string? Weather { get; set; }
        public string? Status { get; set; }
        public string? VolatileStatus { get; set; }
        public double? HpFraction { get; set; }
        public string? PokemonType { get; set; }

        // And / Or
        public MoveCondition? Left { get; set; }
        public MoveCondition? Right { get; set; }

        // Not / UserCondition / OpponentCondition
        public MoveCondition? Inner { get; set; }
        public string? Terrain { get; set; }
        public string? MoveTag { get; set; }
        public string? MoveCategory { get; set; }
        public double? Fraction { get; set; }
    }

    // ── Assembled Effect ─────────────────────────────────────────────────────────
    public class MoveEffect
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Target { get; set; }

        // FormulaDamage / DirectDamage / CrashDamage / Recoil / RestoreHP / Drain
        public MoveNumber? Number { get; set; }
        public string? HealTarget { get; set; }

        // Chance
        public double? ChanceProbability { get; set; }
        public MoveEffect? ChanceChild { get; set; }

        // Conditional
        public MoveCondition? Condition { get; set; }
        public MoveEffect? OnPass { get; set; }
        public MoveEffect? OnFail { get; set; }

        // StatChange
        public string? Stat { get; set; }
        public int? StatStages { get; set; }

        // MultiStatChange
        public List<MultiStatChangeRow> StatChanges { get; set; } = new();

        // Sleep
        public int? SleepMinTurns { get; set; }
        public int? SleepMaxTurns { get; set; }

        // Confuse
        public int? ConfuseMinTurns { get; set; }
        public int? ConfuseMaxTurns { get; set; }

        // Poison
        public bool IsToxic { get; set; }

        // Field
        public string? Weather { get; set; }
        public int? WeatherTurns { get; set; }
        public string? Screen { get; set; }
        public int? ScreenTurns { get; set; }
        public string? BattleSide { get; set; }
        public string? Hazard { get; set; }

        // StoreAndRelease
        public int? ChargeTurns { get; set; }

        // Sequence — ordered child effects
        public List<MoveEffect> SequenceSteps { get; set; } = new();
        public double? Multiplier { get; set; }
        public string? Status { get; set; }

    }

    // ── Assembled Attempt ────────────────────────────────────────────────────────
    public class MoveAttempt
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public double? AccuracyValue { get; set; }

        // Attempt
        public MoveEffect? OnHit { get; set; }
        public MoveEffect? OnMiss { get; set; }
        public MoveEffect? After { get; set; }

        // Cascade
        public bool StopOnMiss { get; set; }
        public List<MoveAttempt> CascadeSteps { get; set; } = new();

        // Combo
        public MoveNumber? HitsNumber { get; set; }

        // Charge
        public MoveEffect? ChargeEffect { get; set; }
        public MoveAttempt? ReleaseAttempt { get; set; }

        // Rampage
        public int? RampageMinTurns { get; set; }
        public int? RampageMaxTurns { get; set; }
        public MoveEffect? AfterRampage { get; set; }
    }

    public class MoveDecorator
    {
        public string Type { get; set; } = "";          // "Precondition" | "Applicability" | "Disable" | "TypeOverride" | "FollowUp"
        public MoveCondition? Condition { get; set; }   // for Precondition
        public MoveCondition? PokemonCondition { get; set; } // for Applicability
        public int? LockTurns { get; set; }             // for Disable
        public string? OverrideType { get; set; }       // for TypeOverride (e.g. "Normal")
        public MoveEffect? FollowUpEffect { get; set; } // for FollowUp
        public string? FailMessage { get; set; }        // optional, for Precondition / Applicability
    }

    // ── Full Move Tree ───────────────────────────────────────────────────────────
    
}