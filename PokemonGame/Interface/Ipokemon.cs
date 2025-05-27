using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Interface
{
    public interface IPokemon
    {
        string Species { get; }
        string Nickname { get; set; }
        int level { get; set; }
        int CurrentHP { get; set; }
        IStatValues IVs { get; }
        IStatValues EVs { get; }
        List<IMove> Moves { get; }
        void TakeDamage(int amount);
        void Heal(int amount);
    }
}
