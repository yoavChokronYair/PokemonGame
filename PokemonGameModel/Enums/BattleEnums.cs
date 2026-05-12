// All battle-state enums used across Domain, Model, and Helper layers.
// Extracted from BattleDomain.cs — do not redefine these elsewhere.
// Used by: BattleDomain, PokemonDomain, IEffect, ICondition, INumber

namespace PokemonGame.Model.Enums
{
    public enum Weather { None, Sun, Rain, Sandstorm, Hail, HeavyRain, HarshSunlight, StrongWinds }
    public enum BattleAction { Move, Switch, Item }
    public enum BattleSide { Attacker, Defender }
    public enum Screen { Reflect, LightScreen, AuroraVeil }
    public enum Hazard { Spikes, ToxicSpikes, StealthRock, StickyWeb }
    public enum Stat { Attack, Defense, SpecialAttack, SpecialDefense, Speed, Accuracy, Evasion,HP }
    public enum StatusCondition { None, Paralysis, Burn, Poison, Toxic, Sleep, Freeze }
    public enum VolatileStatus
    {
        // ── Existing ──────────────────────────────────────────────────────────────
        Confusion,
        Flinch,
        Infatuation,
        Curse,
        LeechSeed,
        SmackDown,
        Ingrain,
        None,

        // ── New — required by Effects.cs ─────────────────────────────────────────
        CritImmune,           // BlockCritical   — suppresses crit roll
        StatProtected,        // PreventStatReduction — value encodes stat (0 = all)
        SecondaryImmune,      // BlockSecondaryEffects
        RecoilImmune,         // BlockRecoil
        IndirectImmune,       // BlockIndirectDamage
        Enduring,             // Endure — survive lethal hit at 1 HP
        SuperEffectiveOnly,   // Wonder Guard — only super-effective moves land
        MaxMultiStrike,       // Skill Link — always max hits on multi-hit moves
        IgnoringStatChanges,  // Mold Breaker stat-ignore (attacker flag)
        Trapped,              // PreventFlee — cannot flee
        CantSwitch,           // PreventSwitch — cannot switch out
        ItemProtected,        // Sticky Hold — item cannot be stolen
        GuaranteedFlee,       // Run Away — flee always succeeds
        EarlyBird,            // ModifySleepTurns — value = multiplier * 10
        Loafing,              // Truant — this turn is a loaf turn
        AbilitySuppressed,    // IgnoreAbility / Mold Breaker on defender
        WeatherSuppressed,    // Cloud Nine / Air Lock
        MoveBlocked,          // Soundproof / Bulletproof etc.
        LiquidOoze,           // DamageRedirect — drain hurts drainer instead
    }
    public enum BotLevel
    {
        Easy, Medium, Hard,
        //game
        Wild,
        BasicTrainer,
        AdvancedTrainer,
        GymLeader,
        EliteFour,
        Champion
    }
    public enum TrainerClass
    {
        BugCatcher,
        Lass,
        Hiker,
        GymLeader,
        EliteFour,
        Champion,
        Rival
    }

}
