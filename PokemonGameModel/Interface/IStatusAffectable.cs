// Layer: Interface — contract definition only, no logic or implementations here.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGame.Enums;

namespace PokemonGame.Interface
{
    public interface IStatusAffectable
    {
        StatusType StatusCondition { get; set; } 
        bool IsFainted { get; set; }
    }
        
}
