using PokemonGame.Model.Enums;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class BattleTurnResolver
    {
        public bool AttackerMovesFirst(PokemonHelper.PokemonState attacker, PokemonHelper.PokemonState defender, int attackerPriority, int defenderPriority)
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