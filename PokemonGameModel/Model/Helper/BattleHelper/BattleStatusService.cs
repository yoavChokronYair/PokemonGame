// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Helper.PokemonHelper;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    internal class BattleStatusService
    {
        private readonly BattleLogger _logger;

        public BattleStatusService(BattleLogger logger) => _logger = logger;

        public void ApplyEndOfTurnStatus(PokemonState pokemon)
        {
            switch (pokemon.PokemonStatusCondition())
            {
                case StatusCondition.Burn:
                    pokemon.TakeDamage(pokemon.MaxHP / 8);
                    _logger.Log($"{pokemon.Name} is hurt by its burn!");
                    break;
                case StatusCondition.Poison:
                    pokemon.TakeDamage(pokemon.MaxHP / 8);
                    _logger.Log($"{pokemon.Name} is hurt by poison!");
                    break;
                case StatusCondition.Toxic:
                    pokemon.ApplyToxicByOne();
                    pokemon.TakeDamage(pokemon.MaxHP * pokemon.getToxicCounter() / 16);
                    _logger.Log($"{pokemon.Name} is hurt by bad poison!");
                    break;
            }
        }
    }
}