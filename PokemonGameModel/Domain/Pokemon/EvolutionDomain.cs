using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Domain.Pokemon
{
    public class EvolutionDomain
    {
        public int PokemonId { get; set; }
        public int ToPokemonId { get; set; }
        public EvoTriggerType TriggerType { get; set; }
        public ICondition<PokemonState> Condition { get; set; }
        public int LevelRequired { get; set; }

    }
    public class LevelUpResult
    {
        public List<int> GainedLevels { get; set; }
            = new();

        public List<MoveLearnResult> LearnedMoves { get; set; }
            = new();

        public bool Evolved { get; set; }

        public int? EvolutionTarget { get; set; }
    }

    public class MoveLearnResult
    {
        public int Level { get; set; }

        public MoveState Move { get; set; } = null!;

        public bool NeedsReplacement { get; set; }
    }
    public class LearnableMove
    {
        public int Level { get; set; }

        public MoveState Move { get; set; }
    }
}
