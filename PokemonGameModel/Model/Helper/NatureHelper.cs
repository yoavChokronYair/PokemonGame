using PokemonGame.Enums;

namespace PokemonGame.Model.Model.Helper
{
    public static class NatureHelper
    {
        private static readonly Dictionary<NatureType, (double atk, double def, double spAtk, double spDef, double speed)> _modifiers
            = new()
        {
            { NatureType.Hardy, (1, 1, 1, 1, 1) },
            { NatureType.Lonely, (1.1, 0.9, 1, 1, 1) },
            { NatureType.Brave, (1.1, 1, 1, 1, 0.9) },
            { NatureType.Adamant, (1.1, 1, 0.9, 1, 1) },
            { NatureType.Naughty, (1.1, 1, 1, 0.9, 1) },

            { NatureType.Bold, (0.9, 1.1, 1, 1, 1) },
            { NatureType.Docile, (1, 1, 1, 1, 1) },
            { NatureType.Relaxed, (1, 1.1, 1, 1, 0.9) },
            { NatureType.Impish, (1, 1.1, 0.9, 1, 1) },
            { NatureType.Lax, (1, 1.1, 1, 0.9, 1) },

            { NatureType.Timid, (0.9, 1, 1, 1, 1.1) },
            { NatureType.Hasty, (1, 0.9, 1, 1, 1.1) },
            { NatureType.Serious, (1, 1, 1, 1, 1) },
            { NatureType.Jolly, (1, 1, 0.9, 1, 1.1) },
            { NatureType.Naive, (1, 1, 1, 0.9, 1.1) },

            { NatureType.Modest, (0.9, 1, 1.1, 1, 1) },
            { NatureType.Mild, (1, 0.9, 1.1, 1, 1) },
            { NatureType.Quiet, (1, 1, 1.1, 1, 0.9) },
            { NatureType.Bashful, (1, 1, 1, 1, 1) },
            { NatureType.Rash, (1, 1, 1.1, 0.9, 1) },

            { NatureType.Calm, (0.9, 1, 1, 1.1, 1) },
            { NatureType.Gentle, (1, 0.9, 1, 1.1, 1) },
            { NatureType.Sassy, (1, 1, 1, 1.1, 0.9) },
            { NatureType.Careful, (1, 1, 0.9, 1.1, 1) },
            { NatureType.Quirky, (1, 1, 1, 1, 1) },
        };

        public static (double atk, double def, double spAtk, double spDef, double speed) GetNatureModifiers(NatureType nature)
        {
            return _modifiers[nature];
        }
    }
}