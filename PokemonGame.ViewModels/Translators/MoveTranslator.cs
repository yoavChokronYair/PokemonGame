using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Handler;

namespace PokemonGame.ViewModels.Translators
{
    // Sits in the ViewModel layer — knows about both the service's MoveTree
    // and the model's domain objects, translating between the two.
    // Neither the model nor the services know this class exists.
    public class MoveTranslator : BaseTranslator
    {
        private readonly IMoveService _moveService;

        public MoveTranslator()
        {
            _moveService = new MoveService();
        }
        public MoveTranslator(IMoveService moveService)
        {
            _moveService = moveService;
        }
        // ── Public entry point ───────────────────────────────────────────────

        public IMove Translate(string moveName)
        {
            var tree = _moveService.GetMove(moveName)
                ?? throw new InvalidOperationException($"Move '{moveName}' not found.");

            if (tree.Attempts.Count == 0)
                throw new InvalidOperationException($"Move '{moveName}' has no attempts.");

            var rootAttemptData = tree.Attempts[0];
            IAttempt translatedAttempt = TranslateAttempt(rootAttemptData);
            var move = tree.Move;

            var moveState = new MoveState(
                attempt: translatedAttempt,
                name: move.Name,
                element: ParseEnum<PokemonType>(move.Element),
                category: ParseEnum<MoveCategory>(move.Category),
                pp: move.PP,
                target: ParseEnum<MoveTarget>(move.Target),
                priority: move.Priority,
                critStage: move.CritStage,
                description: move.Description
            );

            // ← wrap in decorators before returning
            return ApplyDecorators(moveState, tree.Decorators);
        }

        
        public IAttempt TranslateAttempt(MoveAttempt a) => a.Type switch
        {
            "Attempt" => new Attempt(
                accuracy: new Probability(a.AccuracyValue ?? 1.0),
                onHit: a.OnHit != null ? TranslateEffect(a.OnHit) : null,
                onMiss: a.OnMiss != null ? TranslateEffect(a.OnMiss) : null,
                after: a.After != null ? TranslateEffect(a.After) : null),

            "Cascade" => new Cascade(
                attempts: a.CascadeSteps.Select(TranslateAttempt).ToList(),
                stopOnMiss: a.StopOnMiss),

            "Combo" => new Combo(
                accuracy: new Probability(a.AccuracyValue ?? 1.0),
                hits: TranslateNumber(a.HitsNumber!),
                onEachHit: a.OnHit != null ? TranslateEffect(a.OnHit) : new NoEffect(),
                onEachMiss: a.OnMiss != null ? TranslateEffect(a.OnMiss) : null,
                after: a.After != null ? TranslateEffect(a.After) : null),

            "Charge" => new Charge(
                chargeEffect: a.ChargeEffect != null ? TranslateEffect(a.ChargeEffect) : new NoEffect(),
                releaseAttempt: TranslateAttempt(a.ReleaseAttempt!)),

            "Rampage" => new Rampage(
                attack: TranslateAttempt(a.CascadeSteps[0]),
                afterRampage: a.AfterRampage != null ? TranslateEffect(a.AfterRampage) : new NoEffect(),
                minTurns: a.RampageMinTurns ?? 2,
                maxTurns: a.RampageMaxTurns ?? 3),

            _ => throw new NotSupportedException($"Unknown attempt type: '{a.Type}'")
        };

        public IMove ApplyDecorators(IMove move, IReadOnlyList<MoveDecorator> decorators)
        {
            foreach (var d in decorators)
            {
                move = d.Type switch
                {
                    "Precondition" => new WithPrecondition(
                        TranslateCondition(d.Condition!),
                        move,
                        d.FailMessage),

                    "Applicability" => new WithApplicability(
                        TranslatePokemonCondition(d.PokemonCondition!),
                        move,
                        d.FailMessage),

                    "Disable" => new WithDisable(
                        move,
                        d.LockTurns ?? 0),

                    "TypeOverride" => new WithTypeOverride(
                        move,
                        ParseEnum<PokemonType>(d.OverrideType!)),

                    "FollowUp" => new WithFollowUp(
                        move,
                        TranslateEffect(d.FollowUpEffect!)),

                    _ =>  move
                };
            }
            return move;
        }
        
    }
}