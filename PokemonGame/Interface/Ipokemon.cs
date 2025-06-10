using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Interface
{
    public interface IPokemon
    {
        string Species { get; }
        string Nickname { get; set; }
        int Level { get; set; }
        int MaxHP { get; set; }
        IStatValues IVs { get; }
        IStatValues EVs { get; }
        List<IMove> Moves { get; }
        int ID { get;} // Unique PokedexID for the Pokemon instance
        int PokedexID { get; set; } // Unique PokedexID for the Pokemon instance
        bool IsMale { get; set; } // Indicates if the
        bool IsShiny { get; set; } // Indicates if the Pokemon is shiny
    }
}
