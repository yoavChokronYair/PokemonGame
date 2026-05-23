using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Model.DesignPatterns;

namespace PokemonGame.Model.Model.Battle
{
    public class BattleStatusService
    {
        private readonly BattleLogger _logger;

        public BattleStatusService(BattleLogger logger)
        {
            _logger = logger;
        }

        public void ApplyEndOfTurnStatus(BattleState battle, PokemonState pokemon)
        {
            if (pokemon == null)
                return;

            if (pokemon.IsFainted)
                return;

            StatusCondition status = pokemon.PokemonStatusCondition();

            switch (status)
            {
                case StatusCondition.Sleep:
                    ApplySleep(pokemon);
                    break;

                case StatusCondition.Freeze:
                    ApplyFreeze(pokemon);
                    break;

                case StatusCondition.Burn:
                    ApplyBurn(battle, pokemon);
                    break;

                case StatusCondition.Poison:
                    ApplyPoison(battle, pokemon);
                    break;

                case StatusCondition.Toxic:
                    ApplyToxic(battle, pokemon);
                    break;
            }
        }

        private void ApplySleep(PokemonState pokemon)
        {

            bool wokeUp = pokemon.TickSleep();

            if (wokeUp)
            {
                _logger.LogStatus($"{pokemon.Name} woke up!");
            }
        }

        private void ApplyFreeze(PokemonState pokemon)
        {
            if (RandomHelper.Next(0, 5) == 0)
            {
                pokemon.ClearStatus();
                _logger.LogStatus($"{pokemon.Name} thawed out!");
            }
        }

        private void ApplyBurn(BattleState battle, PokemonState pokemon)
        {
            if (IsIndirectDamageBlocked(battle, pokemon))
                return;

            int damage = Math.Max(1, pokemon.MaxHP / 8);

            pokemon.TakeDamage(damage);

            _logger.LogStatus($"{pokemon.Name} is hurt by its burn!");
        }

        private void ApplyPoison(BattleState battle, PokemonState pokemon)
        {
            if (IsIndirectDamageBlocked(battle, pokemon))
                return;

            int damage = Math.Max(1, pokemon.MaxHP / 8);

            pokemon.TakeDamage(damage);

            _logger.LogStatus($"{pokemon.Name} is hurt by poison!");
        }

        private void ApplyToxic(BattleState battle, PokemonState pokemon)
        {
       
            pokemon.ApplyToxicByOne();

            if (IsIndirectDamageBlocked(battle, pokemon))
                return;

            int damage = Math.Max(1, pokemon.MaxHP * pokemon.GetToxicCounter() / 16);

            pokemon.TakeDamage(damage);

            _logger.LogStatus($"{pokemon.Name} is hurt by bad poison!");
        }

        private static bool IsIndirectDamageBlocked(BattleState battle, PokemonState pokemon)
        {
          
            return BlockIndirectDamage.IsActive(battle, pokemon);
        }
    }
}