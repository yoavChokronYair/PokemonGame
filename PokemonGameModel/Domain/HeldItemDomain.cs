using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Domain
{
    public class HeldItemDomain
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsConsumable { get; set; }

    }
}
