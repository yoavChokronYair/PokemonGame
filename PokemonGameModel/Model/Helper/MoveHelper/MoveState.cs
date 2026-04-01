using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;

namespace PokemonGame.Model.Model.Helper.MoveHelper
{
    public class MoveState : IMove
    {
        private readonly MoveDomain _state;
        private readonly IAttempt _attempt;

        public MoveState(MoveDomain state, IAttempt attempt)
        {
            _state = state;
            _attempt = attempt;
        }

        public string Name => _state.Name;
        public PokemonType Element => _state.Element;
        public MoveCategory Category => _state.Category;
        public MoveTarget Target => _state.Target;
        public MoveTag Tag => _state.tag;
        public int PP => _state.PP;
        public int MaxPP => _state.MaxPP;
        public int Priority => _state.Priority;

        public void Execute(BattleState battle)
        {
            if (_state.PP <= 0)
            {
                battle.Logger.Log($"{Name} has no PP left!");
                return;
            }

            _state.PP--;
            battle.RegisterMove(this);
            battle.Logger.Log($"{battle.Attacker.Name} used {Name}!");
            _attempt.Execute(battle);
        }

        public void RestorePP(int amount) => _state.PP = Math.Min(_state.PP + amount, MaxPP);
        public bool HasPP => _state.PP > 0;
    }
}
