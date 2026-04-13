using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;

namespace PokemonGame.Model.Model.Battle
{
    public class BattleTurnResolver
    {
        public bool AttackerMovesFirst(PokemonState attacker, PokemonState defender, int attackerPriority, int defenderPriority)
        {
            // 1. Priority is the absolute tie-breaker
            if (attackerPriority != defenderPriority)
            {
                return attackerPriority > defenderPriority;
            }

            // 2. Calculate modified speeds
            int attackerSpeed = GetModifiedSpeed(attacker);
            int defenderSpeed = GetModifiedSpeed(defender);

            if (attackerSpeed != defenderSpeed)
            {
                return attackerSpeed > defenderSpeed;
            }

            // 3. Speed tie-breaker
            return RandomHelper.NextBool();
        }

        private int GetModifiedSpeed(PokemonState pokemon)
        {
            double multiplier = 1.0;

            // Standard Paralysis speed penalty (with your 25% chance logic preserved)
            if (pokemon.PokemonStatusCondition() == StatusCondition.Paralysis)
            {
                multiplier = 0.75;
            }

            return (int)(pokemon.GetEffectiveStat(Stat.Speed) * multiplier);
        }
    }
}