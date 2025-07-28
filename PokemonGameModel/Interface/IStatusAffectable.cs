using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGameModel.Enums;

namespace PokemonGameModel.Interface
{
    public interface IStatusAffectable
    {
        StatusType StatusCondition { get; set; } 
        bool IsFainted { get; set; }
    }
        
}
