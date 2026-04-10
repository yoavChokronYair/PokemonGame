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

            int attackerSpeed = attacker.GetEffectiveStat(Stat.Speed);
            int defenderSpeed = defender.GetEffectiveStat(Stat.Speed);

            if (attackerSpeed != defenderSpeed)
            {
                return attackerSpeed > defenderSpeed;
            }

            return PokemonGame.Model.Helper.RandomHelper.NextBool();
        }
    }
}