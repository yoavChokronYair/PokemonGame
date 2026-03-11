// Layer: Interface — contract definition only, no logic or implementations here.
﻿using PokemonGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Interface
{
    public interface IMoveResult
    {
         int Damage { get; set; }
         bool IsSwitch { get; set; } 
         StatusType StatusEffect { get; set; } // You can expand this for status names
         int Priority { get; set; }
    }
}
