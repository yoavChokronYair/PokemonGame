using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Helper.BattleHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Model.Helper.MoveHelper
{
    internal class MoveState : IMove
    {
        private readonly MoveDomain state;
        private readonly IAttempt attempt;

        public MoveState(Domain.Move.MoveDomain state, IAttempt attempt)
        {
            this.state = state;
            this.attempt = attempt;
        }

        public string Name => state.Name;
        public PokemonType Element => state.Element;
        public MoveCategory Category => state.Category;
        public MoveTarget Target => state.Target;
        public int PP => state.PP;
        public int MaxPP => state.MaxPP;

        public void Execute(BattleState battle)
        {
            if (state.PP <= 0)
            {
                battle.Logger.Log($"{Name} has no PP left!");
                return;
            }

            state.PP--;
            battle.RegisterMove(this);
            battle.Logger.Log($"{battle.Attacker.Name} used {Name}!");
            attempt.Execute(battle);
        }

        public void RestorePP(int amount) => state.PP = Math.Min(state.PP + amount, MaxPP);
        public bool HasPP => state.PP > 0;
    }
}
