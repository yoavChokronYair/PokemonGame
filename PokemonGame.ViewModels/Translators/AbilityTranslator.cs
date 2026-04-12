using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Services.Data.GameData.PokemonData;
using PokemonGame.Services.Handler;

namespace PokemonGame.ViewModels.Translators
{
    public class AbilityTranslator  :BaseTranslator
    {
        private readonly IAbilityService _abilityService;

        public AbilityTranslator()
        {
            _abilityService = new AbilityService();
        }

        public AbilityTranslator(IAbilityService abilityService, MoveTranslator moveTranslator)
        {
            _abilityService = abilityService;
        }

        // ── Public entry points ──────────────────────────────────────────────

        public AbilityState Translate(string abilityName)
        {
            var tree = _abilityService.GetAbility(abilityName)
                ?? throw new InvalidOperationException($"Ability '{abilityName}' not found.");

            return BuildAbilityState(tree);
        }

        public AbilityState TranslateById(int id)
        {
            var tree = _abilityService.GetAbilityById(id)
                ?? throw new InvalidOperationException($"Ability with id '{id}' not found.");

            return BuildAbilityState(tree);
        }

        // ── Builder ──────────────────────────────────────────────────────────

        private AbilityState BuildAbilityState(AbilityTree tree)
        {
            ICondition<BattleState> condition = tree.Condition != null
             ? TranslateCondition(tree.Condition)
             : new Probability<BattleState>(1.0);   // always passes

            IEffect effect = tree.Effect != null
                ? TranslateEffect(tree.Effect)
                : new NoEffect();
            return new AbilityState(tree.Name, effect,tree.Description);
        }
    }
}