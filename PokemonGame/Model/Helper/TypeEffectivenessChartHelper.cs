using PokemonGame.Enums;
using System.Collections.Generic;
using PokemonGame.Constants;
using System;

namespace PokemonGame.Model.Helper
{
    public static class TypeEffectivenessChartHelper
    {
        private static readonly Dictionary<(PokemonType, PokemonType), double> _chart =
            new Dictionary<(PokemonType, PokemonType), double>()
            {
            // Normal
            { (PokemonType.Normal, PokemonType.Rock), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Normal, PokemonType.Ghost), TypeEffectivenessChartConstants.noEffect },
            { (PokemonType.Normal, PokemonType.Steel), TypeEffectivenessChartConstants.notVeryEffective },

            // Fire
            { (PokemonType.Fire, PokemonType.Grass), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fire, PokemonType.Ice), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fire, PokemonType.Bug), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fire, PokemonType.Steel), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fire, PokemonType.Fire), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fire, PokemonType.Water), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fire, PokemonType.Rock), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fire, PokemonType.Dragon), TypeEffectivenessChartConstants.notVeryEffective },

            // Water
            { (PokemonType.Water, PokemonType.Fire), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Water, PokemonType.Ground), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Water, PokemonType.Rock), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Water, PokemonType.Water), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Water, PokemonType.Grass), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Water, PokemonType.Dragon), TypeEffectivenessChartConstants.notVeryEffective },

            // Grass
            { (PokemonType.Grass, PokemonType.Water), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Grass, PokemonType.Ground), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Grass, PokemonType.Rock), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Grass, PokemonType.Fire), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Grass, PokemonType.Grass), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Grass, PokemonType.Poison), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Grass, PokemonType.Flying), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Grass, PokemonType.Bug), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Grass, PokemonType.Dragon), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Grass, PokemonType.Steel), TypeEffectivenessChartConstants.notVeryEffective },

            // Electric
            { (PokemonType.Electric, PokemonType.Water), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Electric, PokemonType.Flying), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Electric, PokemonType.Electric), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Electric, PokemonType.Grass), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Electric, PokemonType.Dragon), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Electric, PokemonType.Ground), TypeEffectivenessChartConstants.noEffect },

            // Ice
            { (PokemonType.Ice, PokemonType.Grass), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Ice, PokemonType.Ground), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Ice, PokemonType.Flying), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Ice, PokemonType.Dragon), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Ice, PokemonType.Fire), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Ice, PokemonType.Water), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Ice, PokemonType.Ice), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Ice, PokemonType.Steel), TypeEffectivenessChartConstants.notVeryEffective },

            // Fighting
            { (PokemonType.Fighting, PokemonType.Normal), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fighting, PokemonType.Ice), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fighting, PokemonType.Rock), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fighting, PokemonType.Dark), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fighting, PokemonType.Steel), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fighting, PokemonType.Poison), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fighting, PokemonType.Flying), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fighting, PokemonType.Psychic), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fighting, PokemonType.Bug), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fighting, PokemonType.Fairy), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fighting, PokemonType.Ghost), TypeEffectivenessChartConstants.noEffect },

            // Ghost
            { (PokemonType.Ghost, PokemonType.Ghost), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Ghost, PokemonType.Psychic), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Ghost, PokemonType.Dark), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Ghost, PokemonType.Normal), TypeEffectivenessChartConstants.noEffect },

            // Psychic
            { (PokemonType.Psychic, PokemonType.Fighting), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Psychic, PokemonType.Poison), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Psychic, PokemonType.Steel), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Psychic, PokemonType.Psychic), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Psychic, PokemonType.Dark), TypeEffectivenessChartConstants.noEffect },

            // Dark
            { (PokemonType.Dark, PokemonType.Psychic), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Dark, PokemonType.Ghost), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Dark, PokemonType.Dark), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Dark, PokemonType.Fairy), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Dark, PokemonType.Fighting), TypeEffectivenessChartConstants.notVeryEffective },

            // Fairy
            { (PokemonType.Fairy, PokemonType.Fighting), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fairy, PokemonType.Dragon), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fairy, PokemonType.Dark), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Fairy, PokemonType.Fire), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fairy, PokemonType.Steel), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Fairy, PokemonType.Poison), TypeEffectivenessChartConstants.notVeryEffective },

            // Steel
            { (PokemonType.Steel, PokemonType.Rock), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Steel, PokemonType.Ice), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Steel, PokemonType.Fairy), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Steel, PokemonType.Steel), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Steel, PokemonType.Fire), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Steel, PokemonType.Water), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Steel, PokemonType.Electric), TypeEffectivenessChartConstants.notVeryEffective },

            // Dragon
            { (PokemonType.Dragon, PokemonType.Dragon), TypeEffectivenessChartConstants.superEffective },
            { (PokemonType.Dragon, PokemonType.Steel), TypeEffectivenessChartConstants.notVeryEffective },
            { (PokemonType.Dragon, PokemonType.Fairy), TypeEffectivenessChartConstants.noEffect },
            };

        public static double GetEffectiveness(PokemonType attackType, PokemonType defenderType)
        {
            return _chart.TryGetValue((attackType, defenderType), out var multiplier)
                ? multiplier
                : TypeEffectivenessChartConstants.normal;
        }

        public static double GetTotalEffectiveness(PokemonType attackType, PokemonType[] defenderTypes)
        {
            double total = 1.0;
            foreach (var defender in defenderTypes)
                total *= GetEffectiveness(attackType, defender);
            return total;
        }
    }
}
