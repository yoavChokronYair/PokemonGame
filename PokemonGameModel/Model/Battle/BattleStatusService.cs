// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Helper;
using PokemonGame.Model.Model.DesignPatterns;

namespace PokemonGame.Model.Model.Battle
{
    public class BattleStatusService
    {
        private readonly BattleLogger _logger;

        public BattleStatusService(BattleLogger logger) => _logger = logger;

        public void ApplyEndOfTurnStatus(PokemonState pokemon)
        {
            var status = pokemon.PokemonStatusCondition();
            bool magicGuard = BlockIndirectDamage.IsActive(null!, pokemon);

            switch (status)
            {
                case StatusCondition.Sleep:
                    if (RandomHelper.Next(0, 3) == 0)
                    {
                        pokemon.ClearStatus();
                        _logger.Log($"{pokemon.Name} woke up!");
                    }
                    break;

                case StatusCondition.Freeze:
                    if (RandomHelper.Next(0, 5) == 0)
                    {
                        pokemon.ClearStatus();
                        _logger.Log($"{pokemon.Name} thawed out!");
                    }
                    break;

                case StatusCondition.Burn:
                    if (!magicGuard)
                    {
                        pokemon.TakeDamage(pokemon.MaxHP / 8);
                        _logger.Log($"{pokemon.Name} is hurt by its burn!");
                    }
                    break;

                case StatusCondition.Poison:
                    if (!magicGuard)
                    {
                        pokemon.TakeDamage(pokemon.MaxHP / 8);
                        _logger.Log($"{pokemon.Name} is hurt by poison!");
                    }
                    break;

                case StatusCondition.Toxic:
                    if (!magicGuard)
                    {
                        pokemon.ApplyToxicByOne();
                        int damage = pokemon.MaxHP * pokemon.ToxicCounter / 16;
                        pokemon.TakeDamage(damage);
                        _logger.Log($"{pokemon.Name} is hurt by bad poison!");
                    }
                    break;
            }
        }
    }
}