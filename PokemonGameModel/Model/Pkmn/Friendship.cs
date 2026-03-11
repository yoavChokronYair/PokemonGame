// Design: Static lookup table for friendship event bonuses.
// Layer: Model/Pkmn — maps FriendshipEvent to bonus arrays by happiness tier (3 tiers).
// Note: FriendshipEvent enum moved to Enums/PokemonEnum/PokemonEnums.cs.

using PokemonGame.Enums.PokemonEnum;

namespace PokemonGame.Core.Model.Pkmn
{
    internal static class Friendship
    {
        private static sbyte[] GetEventBonuses(FriendshipEvent e)
        {
            switch (e)
            {
                case FriendshipEvent.Walking:         return new sbyte[3] { +2, +2, +1 };
                case FriendshipEvent.LevelUpBattle:   return new sbyte[3] { +5, +4, +3 };
                case FriendshipEvent.Vitamin:         return new sbyte[3] { +5, +3, +2 };
                case FriendshipEvent.Wing:            return new sbyte[3] { +3, +2, +1 };
                case FriendshipEvent.TMHM:            return new sbyte[3] { +1, +1, +0 };
                case FriendshipEvent.BattleItem:      return new sbyte[3] { +1, +1, +0 };
                case FriendshipEvent.Faint_L30:       return new sbyte[3] { -1, -1, -1 };
                case FriendshipEvent.Faint_GE30:      return new sbyte[3] { -5, -5, -10 };
                case FriendshipEvent.Powder:          return new sbyte[3] { -5, -5, -10 };
                case FriendshipEvent.EnergyRoot:      return new sbyte[3] { -10, -10, -15 };
                case FriendshipEvent.RevivalHerb:     return new sbyte[3] { -15, -15, -20 };
                case FriendshipEvent.FriendshipBerry: return new sbyte[3] { +10, +5, +2 };
                case FriendshipEvent.LeagueBattle:    return new sbyte[3] { +5, +4, +3 };
            }
            throw new Exception();
        }
    }
}
