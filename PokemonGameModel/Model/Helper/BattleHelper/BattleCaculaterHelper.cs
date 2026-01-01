using PokemonGame.Constants;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.BattleSystem.Bot;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.Services.Enums.PokemonEnum;
using System;

namespace PokemonGame.Core.Model.Helper.BattleHelper
{
    public class MoveResult 
    {
    //    public int Damage { get; set; }
    //    public bool IsSwitch { get; set; }
    //    public StatusType StatusEffect { get; set; }
    //    public int Priority { get; set; }

    //    public MoveResult()
    //    {
    //        Damage = 0;
    //        IsSwitch = false;
    //        StatusEffect = StatusType.None;
    //        Priority = 0;
    //    }
    //}

    //public static class BattleCalculator
    //{
    //    private static readonly Random _rand = new Random();

    //    // ----------------------------
    //    // Move execution
    //    // ----------------------------
    //    public static MoveResult ExecuteMove(IPokemon defender, IPokemon attacker, MoveData move)
    //    {
    //        var result = new MoveResult();
    //        result.Priority = move.Priority;
    //        switch (move.CategoryEn)
    //        {
    //            case "Physical":
    //            case "Special":
    //                result.Damage = CalculateDamage(defender, attacker, move);
    //                break;

    //            case "Status":
    //                result.StatusEffect = GetStatusFromMove(move);
    //                break;

    //            case "Switch":
    //                result.IsSwitch = true;
    //                break;

    //            default:
    //                break; // Unknown category, no effect
    //        }

    //        return result;
    //    }

    //    // ----------------------------
    //    // Status determination
    //    // ----------------------------
    //    public static StatusType GetStatusFromMove(MoveData move)
    //    {
    //        return move.ename switch
    //        {
    //            "Will‑O‑Wisp" => StatusType.Burn,
    //            "Poison Powder" or "Toxic" or "Poison Gas" => StatusType.Poison,
    //            "Hypnosis" or "Sleep Powder" or "Spore" or "Yawn" => StatusType.Sleep,
    //            "Stun Spore" or "Thunder Wave" or "Body Slam" or "static" => StatusType.Paralysis,
    //            _ => StatusType.None
    //        };
    //    }

    //    // ----------------------------
    //    // Damage calculation
    //    // ----------------------------
    //    private static int CalculateDamage(IPokemon defender, IPokemon attacker, MoveData move)
    //    {
    //        int level = attacker.Level;
    //        int power = move.Power;
    //        PokemonType moveType = move.Type;

    //        int attackStat = GetEffectiveAttack(move.CategoryEn, attacker);
    //        int defenseStat = GetEffectiveDefense(move.CategoryEn, defender);

    //        double baseDamage = (((2 * level / 5.0 + 2) * power * attackStat / defenseStat) / 50.0) + 2;

    //        double effectiveness = TypeEffectivenessChartHelper.GetTotalMoveEffectiveness(moveType, defender.Types);
    //        double stab = attacker.Types.Contains(moveType) ? 1.5 : 1.0;
    //        double crit = IsCriticalHit() ? 1.5 : 1.0;
    //        double randomFactor = _rand.Next(85, 101) / 100.0;

    //        double totalDamage = baseDamage * effectiveness * stab * crit * randomFactor;
    //        return Math.Max(0, (int)Math.Floor(totalDamage));
    //    }

    //    // ----------------------------
    //    // Hit & critical checks
    //    // ----------------------------
    //    public static bool DoesMoveHit(MoveData move, StatusType attackerStatus)
    //    {
    //        if (attackerStatus == StatusType.Sleep || attackerStatus == StatusType.Freeze)
    //            return false;

    //        return _rand.NextDouble() < move.Accuracy / 100.0;
    //    }

    //    public static bool IsCriticalHit() => _rand.NextDouble() < 0.06; // 6% chance

    //    // ----------------------------
    //    // Effective stat calculations
    //    // ----------------------------
    //    public static int GetEffectiveAttack(string moveCategory, IPokemon attacker) =>
    //        moveCategory switch
    //        {
    //            "Physical" => GetEffectivePhysicalAttack(attacker),
    //            "Special" => GetEffectiveSpecialAttack(attacker),
    //            _ => throw new ArgumentException("Invalid move category")
    //        };

    //    public static int GetEffectiveDefense(string moveCategory, IPokemon defender) =>
    //        moveCategory switch
    //        {
    //            "Physical" => GetEffectivePhysicalDefense(defender),
    //            "Special" => GetEffectiveSpecialDefense(defender),
    //            _ => throw new ArgumentException("Invalid move category")
    //        };

    //    private static int GetEffectivePhysicalAttack(IPokemon attacker)
    //    {
    //        double baseAttack = attacker.BaseStats.Attack;
    //        double modifier = NatureHelper.GetNatureModifiers(attacker.Nature).atk;
    //        return (int)Math.Floor(baseAttack * modifier);
    //    }

    //    private static int GetEffectiveSpecialAttack(IPokemon attacker)
    //    {
    //        double baseSpAttack = attacker.BaseStats.SpecialAttack;
    //        double modifier = NatureHelper.GetNatureModifiers(attacker.Nature).spAtk;
    //        return (int)Math.Floor(baseSpAttack * modifier);
    //    }

    //    private static int GetEffectivePhysicalDefense(IPokemon defender)
    //    {
    //        double baseDef = defender.BaseStats.Defense;
    //        double modifier = NatureHelper.GetNatureModifiers(defender.Nature).def;
    //        return (int)Math.Floor(baseDef * modifier);
    //    }

    //    private static int GetEffectiveSpecialDefense(IPokemon defender)
    //    {
    //        double baseSpDef = defender.BaseStats.SpecialDefense;
    //        double modifier = NatureHelper.GetNatureModifiers(defender.Nature).spDef;
    //        return (int)Math.Floor(baseSpDef * modifier);
    //    }

    //    // ----------------------------
    //    // Status bonus for catching
    //    // ----------------------------
    //    private static double GetStatusBonus(IPokemon pokemon) =>
    //        pokemon.StatusType switch
    //        {
    //            StatusType.Sleep => 2.5,
    //            StatusType.Freeze => 2.5,
    //            StatusType.Paralysis => 1.5,
    //            StatusType.Burn => 1.5,
    //            StatusType.Poison => 1.5,
    //            _ => 1.0
    //        };

    //    // ----------------------------
    //    // Catch probability
    //    // ----------------------------
    //    /// <summary>
    //    /// Simulates the 4-shake catch check for a Pokémon.
    //    /// Returns 0 if successfully caught, or the shake number (1-4) that failed.
    //    /// </summary>
    //    //public static int ShakeCheck(WildPokemonBot pokemonBot, PokeballData pokeball)
    //    //{
    //    //    IPokemon pokemon = pokemonBot.activePokemon;
    //    //    int hp = pokemonBot.activePokemonHp;

    //    //    double statusBonus = GetStatusBonus(pokemon);
    //    //    double a = ((3.0 * pokemon.MaxHP - 2.0 * hp) * pokemon. * pokeball.CatchRateModifier) / (3.0 * pokemon.MaxHP);
    //    //    a *= statusBonus;

    //    //    // Auto-catch if a >= 255
    //    //    if (a >= 255)
    //    //        return 0; // success

    //    //    double b = 16711680.0 / a;
    //    //    double shakeThreshold = 1048560.0 / Math.Sqrt(Math.Sqrt(b));

    //    //    // Perform 4 shake checks
    //    //    for (int i = 0; i < 4; i++)
    //    //    {
    //    //        if (_rand.Next(0, 65536) >= shakeThreshold)
    //    //            return i + 1; // shake failed at this number (1–4)
    //    //    }

    //    //    return 0; // caught successfully
    //    //}

    }
}
