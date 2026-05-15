using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.ViewModels.Translators
{
    public class AbilityTranslator : BaseTranslator
    {
        private readonly IAbilityService _abilityService;

        public AbilityTranslator()
        {
            _abilityService = new LocalAbilityService();
        }

        public AbilityTranslator(IAbilityService abilityService)
        {
            _abilityService = abilityService;
        }

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

        private AbilityState BuildAbilityState(AbilityTree tree)
        {
            ICondition<BattleState> condition = tree.Condition != null
                ? TranslateCondition(tree.Condition)
                : new Probability<BattleState>(1.0);

            IEffect effect = tree.Effect != null
                ? TranslateEffect(tree.Effect)
                : new NoEffect();

            return new AbilityState(tree.Name, effect, tree.Description);
        }
    }
    
}