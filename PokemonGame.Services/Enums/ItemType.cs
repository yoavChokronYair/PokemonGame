using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Enums
{
    public enum ItemPouchType : byte
    {
        Items,
        Medicine,
        Balls,
        TMHMs,
        Berries,
        Mail,
        BattleItems,
        KeyItems,
        /// <summary>Accepts all types</summary>
        FreeSpace,
        MAX
    }
}
