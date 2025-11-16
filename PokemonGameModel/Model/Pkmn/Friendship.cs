using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Core.Model.Pkmn
{
    internal static class Friendship
    {
        public enum Event : byte
        {
            Walking,
            LevelUpBattle,
            Vitamin,
            Wing,
            TMHM,
            BattleItem,
            Faint_L30,
            Faint_GE30,
            Powder,
            EnergyRoot,
            RevivalHerb,
            FriendshipBerry,
            LeagueBattle
        }

        private static sbyte[] GetEventBonuses(Event e)
        {
            switch (e)
            {
                case Event.Walking: return new sbyte[3] { +2, +2, +1 };
                case Event.LevelUpBattle: return new sbyte[3] { +5, +4, +3 };
                case Event.Vitamin: return new sbyte[3] { +5, +3, +2 };
                case Event.Wing: return new sbyte[3] { +3, +2, +1 };
                case Event.TMHM: return new sbyte[3] { +1, +1, +0 };
                case Event.BattleItem: return new sbyte[3] { +1, +1, +0 };
                case Event.Faint_L30: return new sbyte[3] { -1, -1, -1 };
                case Event.Faint_GE30: return new sbyte[3] { -5, -5, -10 };
                case Event.Powder: return new sbyte[3] { -5, -5, -10 };
                case Event.EnergyRoot: return new sbyte[3] { -10, -10, -15 };
                case Event.RevivalHerb: return new sbyte[3] { -15, -15, -20 };
                case Event.FriendshipBerry: return new sbyte[3] { +10, +5, +2 };
                case Event.LeagueBattle: return new sbyte[3] { +5, +4, +3 };
            }
            throw new Exception();
        }

       
    }
}