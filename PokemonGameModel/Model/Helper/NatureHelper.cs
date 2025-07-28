using PokemonGameModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGameModel.Model.Helper
{
    public static class NatureHelper
    {
        public static (double atk, double def, double spAtk, double spDef, double speed) GetNatureModifiers(NatureType nature)
        {
            double atk = 1.0, def = 1.0, spAtk = 1.0, spDef = 1.0, speed = 1.0;

            switch (nature)
            {
                case NatureType.Hardy: break;                          // Neutral
                case NatureType.Lonely: atk = 1.1; def = 0.9; break;
                case NatureType.Brave: atk = 1.1; speed = 0.9; break;
                case NatureType.Adamant: atk = 1.1; spAtk = 0.9; break;
                case NatureType.Naughty: atk = 1.1; spDef = 0.9; break;

                case NatureType.Bold: def = 1.1; atk = 0.9; break;
                case NatureType.Docile: break;                        // Neutral
                case NatureType.Relaxed: def = 1.1; speed = 0.9; break;
                case NatureType.Impish: def = 1.1; spAtk = 0.9; break;
                case NatureType.Lax: def = 1.1; spDef = 0.9; break;

                case NatureType.Timid: speed = 1.1; atk = 0.9; break;
                case NatureType.Hasty: speed = 1.1; def = 0.9; break;
                case NatureType.Serious: break;                      // Neutral
                case NatureType.Jolly: speed = 1.1; spAtk = 0.9; break;
                case NatureType.Naive: speed = 1.1; spDef = 0.9; break;

                case NatureType.Modest: spAtk = 1.1; atk = 0.9; break;
                case NatureType.Mild: spAtk = 1.1; def = 0.9; break;
                case NatureType.Quiet: spAtk = 1.1; speed = 0.9; break;
                case NatureType.Bashful: break;                      // Neutral
                case NatureType.Rash: spAtk = 1.1; spDef = 0.9; break;

                case NatureType.Calm: spDef = 1.1; atk = 0.9; break;
                case NatureType.Gentle: spDef = 1.1; def = 0.9; break;
                case NatureType.Sassy: spDef = 1.1; speed = 0.9; break;
                case NatureType.Careful: spDef = 1.1; spAtk = 0.9; break;
                case NatureType.Quirky: break;                       // Neutral
            }

            return (atk, def, spAtk, spDef, speed);
        }
    }

}
