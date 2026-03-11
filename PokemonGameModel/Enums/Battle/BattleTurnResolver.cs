using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Enums.Battle;

namespace PokemonGame.Model.Enums.Battle
{
    internal class BattleTurnResolver
    {
        public bool AttackerMovesFirst(PokemonDomain attacker, PokemonDomain defender, int attackerPriority, int defenderPriority)
        {
            if (attackerPriority != defenderPriority)
                return attackerPriority > defenderPriority;

            int attackerSpeed = attacker.GetEffectiveStat(Stat.Speed);
            int defenderSpeed = defender.GetEffectiveStat(Stat.Speed);

            if (attackerSpeed != defenderSpeed)
                return attackerSpeed > defenderSpeed;

            return PokemonGame.Model.Helper.RandomHelper.NextBool();
        }
    }
}