using PokemonGameModel.Enums;
using PokemonGameModel.Interface;
using PokemonGameModel.Model.BattleSystem.Bot;
using PokemonGameModel.Model.Data;
using PokemonGameModel.Model.Data.Items;
using PokemonGameModel.Model.PokemonCreation;
using System;
using System.Linq;
namespace PokemonGameModel.Model.Helper
{
    public class MoveResult:IMoveResult
    {
        public MoveResult()
        {
            Damage = 0;
            IsSwitch = false;
            StatusEffect = StatusType.None;
        }

        public int Damage { get; set; }
        public bool IsSwitch { get; set; } 
        public StatusType StatusEffect { get; set; }// You can expand this for status names
    }
    public static class BattleCalculator
    {
        private static readonly Random _rand = new Random();
        public static MoveResult result = new MoveResult();
        // Result of move execution
        public static MoveResult ExecuteMove(IPokemon defender, IPokemon attacker, MoveData move)
        {

            switch (move.CategoryEn)
            {
                case "Physical":
                case "Special":
                    CalculateDamage(defender, attacker, move);
                    result.StatusEffect = StatusType.None;
                    result.IsSwitch = false;
                    break;

                case "Status":
                    result.Damage = 0;
                    result.StatusEffect = GetStatusFromPokemon(move);
                    result.IsSwitch = false;
                    break;

                case "Switch":
                    result.Damage = 0;
                    result.IsSwitch = true;
                    result.StatusEffect = StatusType.None;
                    break;

                default:
                    // Unknown category, no effect
                    result.Damage = 0;
                    break;
            }

            return result;
        }
        public static StatusType GetStatusFromPokemon(MoveData move)
        {
            //ToDo:Make sure every move activets in the same turn
            if(move.ename == "Will‑O‑Wisp") {
                return StatusType.Burn;
            }
            if(move.ename == "Poison Powder" || move.ename == "Toxic" || move.ename == "Poison Gas")
            {
                return StatusType.Poison;
            }
            if (move.ename == "Hypnosis" || move.ename == "Sleep Powder" || move.ename == "Spore" || move.ename == "Yawn")
            {
                return StatusType.Sleep;
            }
            if (move.ename == "Stun Spore" || move.ename == "Thunder Wave" || move.ename == "Body Slam" || move.ename == "static")
            {
                return StatusType.Paralysis;
            }
            return StatusType.None;           
        }

        private static void CalculateDamage(IPokemon defender, IPokemon attacker, MoveData move)
        {
            int level = attacker.Level;
            int power = move.Power;
            PokemonType moveType = move.Type;

            // Determine attack and defense stats based on move category
            int attackStat = GetEffectiveAttack(move.CategoryEn, attacker);
            int defenseStat = GetEffectiveDefense(move.CategoryEn, defender);

            // Base damage formula
            double baseDamage = (((2 * level / 5.0 + 2) * power * attackStat / defenseStat) / 50.0) + 2;

            // Type effectiveness
            double effectiveness = TypeEffectivenessChartHelper.GetTotalEffectiveness(moveType, defender.Types);

            // STAB (Same Type Attack Bonus)
            bool hasSTAB = attacker.Types.Contains(moveType);
            double stab = hasSTAB ? 1.5 : 1.0;

            // Critical hit multiplier
            double crit = IsCriticalHit() ? 1.5 : 1.0;

            // Random factor (85–100%)
            double random = _rand.Next(85, 101) / 100.0;

            // Calculate total damage
            double totalDamage = baseDamage * effectiveness * stab * crit * random;
            result.Damage = (int)totalDamage;
        }

        // Other methods unchanged:
        public static bool DoesMoveHit(MoveData move)
        {
            if(result.StatusEffect == StatusType.Sleep || result.StatusEffect == StatusType.Freeze) return false;
            
            return _rand.Next(0, 100) < move.Accuracy;
        }

        public static bool IsCriticalHit()
        {
            return _rand.Next(0, 100) < 6; // 6% chance
        }

        public static int GetEffectiveAttack(string moveCategory, IPokemon attacker)
        {
            if (moveCategory == "Physical")
                return GetEffectivePhysicalAttack(attacker);
            else if (moveCategory == "Special")
                return GetEffectiveSpAttack(attacker);
            else
                throw new ArgumentException("Invalid move category");
        }

        public static int GetEffectivePhysicalAttack(IPokemon attacker)
        {
            // Example: get base attack + modifiers like nature, buffs, etc.
            int baseAttack = attacker.IVs.Attack;
            double natureMod = NatureHelper.GetNatureModifiers(attacker.Nature).atk;
            double otherModifiers = 1.0; // Add buffs/debuffs here

            double effectiveAttack = baseAttack * natureMod * otherModifiers;
            return (int)Math.Floor(effectiveAttack);
        }

        public static int GetEffectiveSpAttack(IPokemon attacker)
        {
            // Example: get base special attack + modifiers
            int baseSpAttack = attacker.IVs.SpecialAttack;
            double natureMod = NatureHelper.GetNatureModifiers(attacker.Nature).spAtk;
            double otherModifiers = 1.0; // Add buffs/debuffs here

            double effectiveSpAttack = baseSpAttack * natureMod * otherModifiers;
            return (int)Math.Floor(effectiveSpAttack);
        }


        public static int GetEffectiveDefense(string moveCategory, IPokemon defender)
        {
            if (moveCategory == "Physical")
                return GetEffectivePhysicalDefense(defender);
            else if (moveCategory == "Special")
                return GetEffectiveSpDefense(defender);
            else
                throw new ArgumentException("Invalid move category");
        }

        public static int GetEffectiveSpDefense(IPokemon defender)
        {
            int baseSpDef = defender.IVs.SpecialDefense;
            double natureMod = NatureHelper.GetNatureModifiers(defender.Nature).spDef;
            double otherModifiers = 1.0; // Add buffs/debuffs here if needed

            double effectiveSpDef = baseSpDef * natureMod * otherModifiers;
            return (int)Math.Floor(effectiveSpDef);
        }
        public static int GetEffectivePhysicalDefense(IPokemon defender)
        {
            int baseDef = defender.IVs.Defense;
            double natureMod = NatureHelper.GetNatureModifiers(defender.Nature).def;
            double otherModifiers = 1.0; // Add buffs/debuffs here if needed

            double effectiveDef = baseDef * natureMod * otherModifiers;
            return (int)Math.Floor(effectiveDef);
        }
        public static double GetStatusBonus(IPokemon pokemon)
        {
            switch (pokemon.StatusType)
            {
                case StatusType.Sleep:
                     return 2.5;    
                case StatusType.Freeze:
                     return 2.5;
                case StatusType.Paralysis:
                    return 1.5;
                case StatusType.Burn:
                    return 1.5;
                case StatusType.Poison:
                    return 1.5;
                default:
                    return 1;
            }
        }
        public static bool IsCaught(WildPokemonBot pokemonBot,PokeballData pokeball)
        {
            IPokemon pokemon = pokemonBot._ActivePokemon;
            int hp = pokemonBot._ActivePokemonHp;
            double statusBonus = GetStatusBonus(pokemon);
            double a = ((3 * pokemon.MaxHP - 2 * hp) * pokemon.CatchRate * pokeball.CatchRateModifier) / (3 * pokemon.MaxHP);
            a *= statusBonus;

            // Auto-catch if a >= 255
            if (a >= 255)
            {
                return true;
            }

            // Step 2: Calculate shake probability using b
            double catchRateFactor = 16711680.0 / a;
            double b = 1048560.0 / Math.Sqrt(Math.Sqrt(catchRateFactor));

            Random rng = new Random();

            // Shake checks (4 times)
            for (int i = 0; i < 4; i++)
            {
                int roll = rng.Next(0, 65536); // 0–65535
                if (roll >= b)
                {
                    return false; // Broke free
                }
            }

            return true; // All 4 shakes passed — caught!
        }
    }
}

