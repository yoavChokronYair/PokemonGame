using PokemonGame.Interface;
using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PokemonGame.Model.Domain.Move
{
    internal class MoveDomain : IMove
    {
        public IAttampt attampt { get; set; }
        public string name;
        public PokemonType element;

        public MoveDomain(string name, PokemonType element,IAttampt attampt)
        {
            this.attampt = attampt;
            this.name = name;
            this.element = element;
        }

        public void Execute(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    //wrapper

    internal class WithPrecondition : IMove
    {
        public ICondition<BattleDomain> condition { get; set; }
        public IMove move;

        public void Execute(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
    //wrapper
    internal class WithApplicability : IMove
    {
        public ICondition<PokemonData> condition { get; set; }
        public IMove move;

        public void Execute(BattleDomain battle)
        {
            throw new NotImplementedException();
        }
    }
}
