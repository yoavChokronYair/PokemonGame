// All battle-state enums used across Domain, Model, and Helper layers.
// Extracted from BattleDomain.cs — do not redefine these elsewhere.
// Used by: BattleDomain, PokemonDomain, IEffect, ICondition, INumber

namespace PokemonGame.Model.Enums
{
    public enum Weather { None, Sun, Rain, Sandstorm, Hail, HeavyRain, HarshSunlight, StrongWinds }
    public enum BattleSide { Attacker, Defender }
    public enum Screen { Reflect, LightScreen, AuroraVeil }
    public enum Hazard { Spikes, ToxicSpikes, StealthRock, StickyWeb }
    public enum Stat { Attack, Defense, SpecialAttack, SpecialDefense, Speed, Accuracy, Evasion }
    public enum StatusCondition { None, Paralysis, Burn, Poison, Toxic, Sleep, Freeze }
    public enum VolatileStatus { Confusion, Flinch, Infatuation, Curse, LeechSeed, SmackDown, Ingrain,None }
}
