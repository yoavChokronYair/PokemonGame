// Layer: Interface — contract definition only, no logic or implementations here.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGame.Interface
{
    public interface ICatchable
    {
        double CatchRate { get; } // 0.0 to 1.0
        bool IsCaught { get; set; }
    }   
}
