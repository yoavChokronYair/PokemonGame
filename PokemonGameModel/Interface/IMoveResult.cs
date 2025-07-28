using PokemonGameModel.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGameModel.Interface
{
    public interface IMoveResult
    {
         int Damage { get; set; }
         bool IsSwitch { get; set; } 
         StatusType StatusEffect { get; set; } // You can expand this for status names
    }
}
