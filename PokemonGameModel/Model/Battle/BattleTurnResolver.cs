using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Battle
{
    public class BattleTurnResolver
    {
        public bool AttackerMovesFirst(PokemonState attacker, PokemonState defender, int attackerPriority, int defenderPriority)
        {
            if (attackerPriority != defenderPriority)
            {
                return attackerPriority > defenderPriority;
            }

            int attackerSpeed = GetModifiedSpeed(attacker);
            int defenderSpeed = GetModifiedSpeed(defender);

            if (attackerSpeed != defenderSpeed)
            {
                return attackerSpeed > defenderSpeed;
            }

            return PokemonGame.Model.Helper.RandomHelper.NextBool();
        }

        private static int GetModifiedSpeed(PokemonState pokemon)
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