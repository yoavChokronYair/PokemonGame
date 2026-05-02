using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Config
{
    public static class TimingConfig
    {
        public static TimeSpan NpcTickInterval = TimeSpan.FromMilliseconds(500);
        public static TimeSpan AutoSaveInterval = TimeSpan.FromMinutes(5);
    }
}
