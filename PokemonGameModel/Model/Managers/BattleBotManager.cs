using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Interface;

namespace PokemonGame.Model.Model.Managers
{
    internal class BattleBotManager
    {
        public IMove PickBotMove(PokemonState bot) => bot.Moves[0];

    }


}
