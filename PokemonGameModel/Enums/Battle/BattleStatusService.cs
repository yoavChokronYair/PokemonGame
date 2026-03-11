// Design: Aggregate Root for a single battle (holds both sides, weather, turn count).
// Layer: Domain — processed battle state; no SQLite, no UI.
// OOP: Encapsulation — all mutation through public methods; sides exposed as read-only.
// Note: All enums (Weather, Screen, Stat, etc.) live in Enums/Battle/BattleEnums.cs.
// BattleSideState is kept here as it is tightly coupled to BattleDomain.

using PokemonGame.Enums.Battle;
using PokemonGame.Model.Domain.Pokemon;

namespace PokemonGame.Model.Enums.Battle
{
    internal class BattleStatusService
    {
        private readonly BattleLogger logger;

        public BattleStatusService(BattleLogger logger) => this.logger = logger;

        public void ApplyEndOfTurnStatus(PokemonDomain pokemon)
        {
            switch (pokemon.Status)
            {
                case StatusCondition.Burn:
                    pokemon.TakeDamage(pokemon.MaxHP / 8);
                    logger.Log($"{pokemon.Name} is hurt by its burn!");
                    break;
                case StatusCondition.Poison:
                    pokemon.TakeDamage(pokemon.MaxHP / 8);
                    logger.Log($"{pokemon.Name} is hurt by poison!");
                    break;
                case StatusCondition.Toxic:
                    pokemon.ToxicCounter++;
                    pokemon.TakeDamage(pokemon.MaxHP * pokemon.ToxicCounter / 16);
                    logger.Log($"{pokemon.Name} is hurt by bad poison!");
                    break;
            }
        }
    }
}