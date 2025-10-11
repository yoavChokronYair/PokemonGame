using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Interface
{
    public interface IEvolvable
    {
        bool CanEvolve { get; }
        void Evolve();
    }
}
