using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Battle
{
    public class BattleTurnResolver
    {
        public bool AttackerMovesFirst(
            PokemonState attacker,
            PokemonState defender,
            int attackerPriority,
            int defenderPriority)
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
            int speed = pokemon.GetEffectiveStat(Stat.Speed);

            // FireRed / Gen III:
            // Paralysis reduces Speed to 25% of normal.
            // The 25% "fully paralyzed" chance belongs in move execution,
            // not in speed calculation.
            if (pokemon.PokemonStatusCondition() == StatusCondition.Paralysis)
            {
                speed /= 4;
            }

            return Math.Max(1, speed);
        }
    }
}