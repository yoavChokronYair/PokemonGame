// Design: Static calculator (pure functions, no state).
// Layer: Model/Helper/BattleHelper — damage formula, hit checks, catch probability.
// Used by: RivalBot, WildPokemonBot, PlayerPokemonBot.
// Note: MoveResult class moved to Domain/Battle/MoveResult.cs (implements IMoveResult).
// Status: all calculation logic is commented out pending refactor to PokemonDomain.

using PokemonGame.Constants;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Core.Model.Helper.BattleHelper
{
    public static class BattleCalculator
    {
        // All logic is commented out — MoveResult is now in Domain/Battle/MoveResult.cs

        //private static readonly Random _rand = new Random();

        //public static MoveResult ExecuteMove(IPokemon defender, IPokemon attacker, MoveData move)
        //{
        //    var result = new MoveResult();
        //    result.Priority = move.Priority;
        //    switch (move.CategoryEn)
        //    {
        //        case "Physical":
        //        case "Special":
        //            result.Damage = CalculateDamage(defender, attacker, move);
        //            break;
        //        case "Status":
        //            result.StatusEffect = GetStatusFromMove(move);
        //            break;
        //        case "Switch":
        //            result.IsSwitch = true;
        //            break;
        //    }
        //    return result;
        //}

        //public static StatusType GetStatusFromMove(MoveData move)
        //{
        //    return move.ename switch
        //    {
        //        "Will-O-Wisp" => StatusType.Burn,
        //        "Poison Powder" or "Toxic" or "Poison Gas" => StatusType.Poison,
        //        "Hypnosis" or "Sleep Powder" or "Spore" or "Yawn" => StatusType.Sleep,
        //        "Stun Spore" or "Thunder Wave" or "Body Slam" or "static" => StatusType.Paralysis,
        //        _ => StatusType.None
        //    };
        //}

        //private static int CalculateDamage(IPokemon defender, IPokemon attacker, MoveData move)
        //{
        //    int level = attacker.Level;
        //    int power = move.Power;
        //    PokemonType moveType = move.Type;
        //    int attackStat = GetEffectiveAttack(move.CategoryEn, attacker);
        //    int defenseStat = GetEffectiveDefense(move.CategoryEn, defender);
        //    double baseDamage = (((2 * level / 5.0 + 2) * power * attackStat / defenseStat) / 50.0) + 2;
        //    double effectiveness = TypeEffectivenessChartHelper.GetTotalMoveEffectiveness(moveType, defender.Types);
        //    double stab = attacker.Types.Contains(moveType) ? 1.5 : 1.0;
        //    double crit = IsCriticalHit() ? 1.5 : 1.0;
        //    double randomFactor = _rand.Next(85, 101) / 100.0;
        //    double totalDamage = baseDamage * effectiveness * stab * crit * randomFactor;
        //    return Math.Max(0, (int)Math.Floor(totalDamage));
        //}

        //public static bool DoesMoveHit(MoveData move, StatusType attackerStatus)
        //{
        //    if (attackerStatus == StatusType.Sleep || attackerStatus == StatusType.Freeze)
        //        return false;
        //    return _rand.NextDouble() < move.Accuracy / 100.0;
        //}

        //public static bool IsCriticalHit() => _rand.NextDouble() < 0.06;
    }
}
