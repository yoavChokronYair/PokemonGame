using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Interface
{
    public interface IBattleParticipant
    {
        void PerformMove(int moveIndex, IPokemon target);
        void ReceiveDamage(int amount);
        void ApplyStatusEffect(string effect);
    }

}
