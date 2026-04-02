// Design: Value Object / Calculator — computes all final stats once and caches them as properties.
// Layer: Model/Helper/MathHelper — Gen 3+ stat formula (HP and non-HP stats, nature modifier).
// CANONICAL stat calculator — PokemonDomain delegates to this class (no duplicate formulas elsewhere).
// Uses NatureHelper.GetNatureModifiers for nature modifier lookups.
using PokemonGame.Enums;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Model;
using PokemonGame.Model.Model.Helper;
using PokemonGame.Model.Model.Helper.BattleHelper;
using PokemonGame.Model.Model.Helper.MoveHelper;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Core.Model.Helper.MathHelper
{
    public class PokemonStatCalculatorHelper
    {
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpecialAttack { get; set; }
        public int SpecialDefense { get; set; }
        public int Speed { get; set; }
        public PokemonStatCalculatorHelper(
            int baseHP, int baseAttack, int baseDefense, int baseSpecialAttack, int baseSpecialDefense, int baseSpeed,
            int ivHP, int ivAttack, int ivDefense, int ivSpecialAttack, int ivSpecialDefense, int ivSpeed,
            int evHP, int evAttack, int evDefense, int evSpecialAttack, int evSpecialDefense, int evSpeed,
            int level, NatureType nature)
        {
            ValidateIVs(ivHP, ivAttack, ivDefense, ivSpecialAttack, ivSpecialDefense, ivSpeed);
            ValidateEVs(evHP, evAttack, evDefense, evSpecialAttack, evSpecialDefense, evSpeed);

            var natureModifiers = NatureHelper.GetNatureModifiers(nature);

            this.HP = CalculateHP(baseHP, ivHP, evHP, level);
            this.Attack = CalculateStat(baseAttack, ivAttack, evAttack, level, natureModifiers.atk);
            this.Defense = CalculateStat(baseDefense, ivDefense, evDefense, level, natureModifiers.def);
            this.SpecialAttack = CalculateStat(baseSpecialAttack, ivSpecialAttack, evSpecialAttack, level, natureModifiers.spAtk);
            this.SpecialDefense = CalculateStat(baseSpecialDefense, ivSpecialDefense, evSpecialDefense, level, natureModifiers.spDef);
            this.Speed = CalculateStat(baseSpeed, ivSpeed, evSpeed, level, natureModifiers.speed);
        }
        private static void ValidateEVs(params int[] evs)
        {
            int total = evs.Sum();

            foreach (var ev in evs)
            {
                if (ev < 0 || ev > 255)
                {
                    throw new ArgumentOutOfRangeException(nameof(evs), "Each EV must be between 0 and 255.");
                }
            }

            if (total > 510)
            {
                throw new ArgumentOutOfRangeException(nameof(evs), "Total EVs cannot exceed 510.");
            }
        }
        private static void ValidateIVs(params int[] ivs)
        {
            foreach (var iv in ivs)
            {
                if (iv < 0 || iv > 31)
                {
                    throw new ArgumentOutOfRangeException(nameof(ivs), "Each IV must be between 0 and 31.");
                }
            }
        }
        public static int CalculateHP(int baseStat, int iv, int ev, int level)
        {
            int evContribution = ev / 4; // floor division
            return ((2 * baseStat + iv + evContribution) * level) / 100 + level + 10;
        }
        public static int CalculateStat(int baseStat, int iv, int ev, int level, double natureModifier)
        {
            int evContribution = ev / 4; // floor division
            int baseValue = ((2 * baseStat + iv + evContribution) * level) / 100 + 5;
            return (int)Math.Floor(baseValue * natureModifier);
        }
        public static int PokemonDamageFormulaCaculator(BattleState Battle, int basePower)
        {
            var move = (MoveState)Battle.LastUsedMove;
            var attacker = Battle.Attacker;
            var defender = Battle.Defender;

            double modifier = getStabBonus(attacker, move.Element) *
                TypeEffectivenessChartHelper.GetTotalMoveEffectiveness(move.Element, defender.GetPokemonTypes(), Battle.Logger) *
                RNGHelper.getCritModifier(Battle.Logger) *
                RandomHelper.NextDouble(0.85, 1.0) *
                GetHeldItemAndAbilityModifier(Battle, move, basePower);

            double levelFactor = ((2.0 * attacker.Level) + 10) / 250;

            double statRatio = move.Category switch
            {
                MoveCategory.Physical => (double)attacker.Attack / defender.Defense,
                MoveCategory.Special => (double)attacker.SpAttack / defender.SpDefense,
                _ => 1.0
            };

            double baseDamage = (levelFactor * basePower * statRatio) + 2.0;
            double finalDamage = baseDamage * modifier;
            return (int)Math.Floor(PokemonGame.Model.Helper.MathHelper.Clamp(finalDamage, 1, 32678));
        }
        public static double getStabBonus(PokemonState pokemon, PokemonType moveType)
        {
            return pokemon.HasType(moveType) ? 1.5 : 1.0;
        }
        public static double GetHeldItemAndAbilityModifier(BattleState battle, MoveState move, double BasePower)
        {
            var attacker = battle.Attacker;
            var defender = battle.Defender;
            double modifier = 1.0;

            // ── Held Item Modifiers ───────────────────────────────────────────────────
            if (attacker.HeldItem is HeldItemState item)
            {
                // Life Orb - 1.3x all damage
                if (item.Name == "Life Orb")
                {
                    modifier *= 1.3;
                }

                // Choice Band - 1.5x physical
                else if (item.Name == "Choice Band" && move.Category == MoveCategory.Physical)
                {
                    modifier *= 1.5;
                }

                // Choice Specs - 1.5x special
                else if (item.Name == "Choice Specs" && move.Category == MoveCategory.Special)
                {
                    modifier *= 1.5;
                }

                // Expert Belt - 1.2x on super effective
                else if (item.Name == "Expert Belt" &&
                    TypeEffectivenessChartHelper.GetTotalMoveEffectiveness(move.Element, defender.GetPokemonTypes(), battle.Logger) > 1.0)
                {
                    modifier *= 1.2;
                }

                // Wise Glasses - 1.1x special
                else if (item.Name == "Wise Glasses" && move.Category == MoveCategory.Special)
                {
                    modifier *= 1.1;
                }

                // Muscle Band - 1.1x physical
                else if (item.Name == "Muscle Band" && move.Category == MoveCategory.Physical)
                {
                    modifier *= 1.1;
                }

                // Type boosters - 1.2x matching type
                else if (GetTypeBoosterType(item.Name) is PokemonType boostedType && move.Element == boostedType)
                {
                    modifier *= 1.2;
                }
            }

            // ── Ability Modifiers ─────────────────────────────────────────────────────
            if (attacker.Ability is AbilityState ability)
            {
                // Overgrow / Blaze / Torrent / Swarm - 1.5x when HP < 1/3
                if (ability.Name is "Overgrow" or "Blaze" or "Torrent" or "Swarm")
                {
                    PokemonType boostedType = ability.Name switch
                    {
                        "Overgrow" => PokemonType.Grass,
                        "Blaze" => PokemonType.Fire,
                        "Torrent" => PokemonType.Water,
                        "Swarm" => PokemonType.Bug,
                        _ => PokemonType.None
                    };
                    if (move.Element == boostedType && attacker.GetHPFraction() < 0.33)
                    {
                        modifier *= 1.5;
                    }
                }

                // Technician - 1.5x moves with base power <= 60
                else if (ability.Name == "Technician" && BasePower <= 60)
                {
                    modifier *= 1.5;
                }

                // Adaptability - 2.0x STAB instead of 1.5x (applied as extra on top of getStabBonus)
                else if (ability.Name == "Adaptability" && attacker.HasType(move.Element))
                {

                    modifier *= (2.0 / 1.5); // neutralize normal STAB and apply 2x
                }

                // Tinted Lens - 2x not very effective moves
                else if (ability.Name == "Tinted Lens" &&
                    TypeEffectivenessChartHelper.GetTotalMoveEffectiveness(move.Element, defender.GetPokemonTypes(),battle.Logger) < 1.0)
                {
                    modifier *= 2.0;
                }

                // Hustle - 1.5x physical attack
                else if (ability.Name == "Hustle" && move.Category == MoveCategory.Physical)
                {
                    modifier *= 1.5;
                }

                // Guts - 1.5x Attack when statused (burn Attack halving is already applied in GetEffectiveStat,
                // so add extra 1.5x * 2 to compensate and apply Guts bonus)
                else if (ability.Name == "Guts" && move.Category == MoveCategory.Physical &&
                    attacker.PokemonStatusCondition() != StatusCondition.None)
                {
                    modifier *= 1.5;
                    if (attacker.PokemonStatusCondition() == StatusCondition.Burn)
                    {
                        modifier *= 2.0; // undo burn halving that was already applied in GetEffectiveStat
                    }
                }

                // Sheer Force - 1.3x if move has secondary effect (NoEffect moves won't have child_effect)
                // handled at move level — skip here unless you track it on MoveState

                // Filter / Solid Rock - 0.75x damage received when super effective (defender ability)
                else if (defender.Ability is AbilityState defAbility &&
                    defAbility.Name is "Filter" or "Solid Rock" &&
                    TypeEffectivenessChartHelper.GetTotalMoveEffectiveness(move.Element, defender.GetPokemonTypes(), battle.Logger) > 1.0)
                {
                    modifier *= 0.75;
                }
            }

            return modifier;
        }

        private static PokemonType? GetTypeBoosterType(string itemName) => itemName switch
        {
            "Silk Scarf" => PokemonType.Normal,
            "Black Belt" => PokemonType.Fighting,
            "Sharp Beak" => PokemonType.Flying,
            "Poison Barb" => PokemonType.Poison,
            "Soft Sand" => PokemonType.Ground,
            "Hard Stone" => PokemonType.Rock,
            "Silver Powder" => PokemonType.Bug,
            "Spell Tag" => PokemonType.Ghost,
            "Metal Coat" => PokemonType.Steel,
            "Charcoal" => PokemonType.Fire,
            "Mystic Water" => PokemonType.Water,
            "Miracle Seed" => PokemonType.Grass,
            "Magnet" => PokemonType.Electric,
            "Twisted Spoon" => PokemonType.Psychic,
            "Never-Melt Ice" => PokemonType.Ice,
            "Dragon Fang" => PokemonType.Dragon,
            "Black Glasses" => PokemonType.Dark,
            _ => null
        };

    }
}
