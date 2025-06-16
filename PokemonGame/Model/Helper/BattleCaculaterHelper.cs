using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.PokemonCreation;
using System;
using System.Linq;

namespace PokemonGame.Model.Helper
{
    public class BattleCaculater
    {
        public static class BattleCalculator
        {
            private static readonly Random _rand = new Random();

            // Result of move execution
            public class MoveResult
            {
                public int Damage { get; set; }
                public bool IsSwitch { get; set; } = false;
                public bool IsStatusMove { get; set; } = false;
                public string StatusEffect { get; set; } = null; // You can expand this for status names
            }

            public static MoveResult ExecuteMove(EnemyPokemonGeneration defender, PlayerPokemonGeneration attacker, MoveData move)
            {
                var result = new MoveResult();

                switch (move.CategoryEn)
                {
                    case "Physical":
                    case "Special":
                        result.Damage = CalculateDamage(defender, attacker, move);
                        break;

                    case "Status":
                        result.Damage = 0;
                        result.IsStatusMove = true;
                        // TODO: Implement status effect application here, example:
                        // result.StatusEffect = move.StatusEffectName;
                        break;

                    case "Switch":
                        result.Damage = 0;
                        result.IsSwitch = true;
                        // TODO: Implement switch logic outside this method
                        break;

                    default:
                        // Unknown category, no effect
                        result.Damage = 0;
                        break;
                }

                return result;
            }

            private static int CalculateDamage(EnemyPokemonGeneration defender, PlayerPokemonGeneration attacker, MoveData move)
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

                return Math.Max(1, (int)Math.Floor(totalDamage)); // minimum damage is 1
            }

            // Other methods unchanged:
            public static bool DoesMoveHit(IMove move)
            {
                return _rand.Next(0, 100) < move.Accuracy;
            }

            public static bool IsCriticalHit()
            {
                return _rand.Next(0, 100) < 6; // 6% chance
            }

            public static int GetEffectiveAttack(string moveCategory, PlayerPokemonGeneration attacker)
            {
                if (moveCategory == "Physical")
                    return GetEffectivePhysicalAttack(attacker);
                else if (moveCategory == "Special")
                    return GetEffectiveSpAttack(attacker);
                else
                    throw new ArgumentException("Invalid move category");
            }

            public static int GetEffectivePhysicalAttack(PlayerPokemonGeneration attacker)
            {
                // Example: get base attack + modifiers like nature, buffs, etc.
                int baseAttack = attacker.IVs.Attack;
                double natureMod = NatureHelper.GetNatureModifiers(attacker.Nature).atk;
                double otherModifiers = 1.0; // Add buffs/debuffs here

                double effectiveAttack = baseAttack * natureMod * otherModifiers;
                return (int)Math.Floor(effectiveAttack);
            }

            public static int GetEffectiveSpAttack(PlayerPokemonGeneration attacker)
            {
                // Example: get base special attack + modifiers
                int baseSpAttack = attacker.IVs.SpecialAttack;
                double natureMod = NatureHelper.GetNatureModifiers(attacker.Nature).spAtk;
                double otherModifiers = 1.0; // Add buffs/debuffs here

                double effectiveSpAttack = baseSpAttack * natureMod * otherModifiers;
                return (int)Math.Floor(effectiveSpAttack);
            }


            public static int GetEffectiveDefense(string moveCategory, EnemyPokemonGeneration defender)
            {
                if (moveCategory == "Physical")
                    return GetEffectivePhysicalDefense(defender);
                else if (moveCategory == "Special")
                    return GetEffectiveSpDefense(defender);
                else
                    throw new ArgumentException("Invalid move category");
            }

            public static int GetEffectiveSpDefense(EnemyPokemonGeneration defender)
            {
                int baseSpDef = defender.IVs.SpecialDefense;
                double natureMod = NatureHelper.GetNatureModifiers(defender.nature).spDef;
                double otherModifiers = 1.0; // Add buffs/debuffs here if needed

                double effectiveSpDef = baseSpDef * natureMod * otherModifiers;
                return (int)Math.Floor(effectiveSpDef);
            }
            public static int GetEffectivePhysicalDefense(EnemyPokemonGeneration defender)
            {
                int baseDef = defender.IVs.Defense;
                double natureMod = NatureHelper.GetNatureModifiers(defender.nature).def;
                double otherModifiers = 1.0; // Add buffs/debuffs here if needed

                double effectiveDef = baseDef * natureMod * otherModifiers;
                return (int)Math.Floor(effectiveDef);
            }
        }
    }
}
