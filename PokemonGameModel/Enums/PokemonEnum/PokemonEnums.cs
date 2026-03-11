// Pokemon-level enums that did not fit existing enum files.
// AbilityType extracted from PartyPokemon.cs.
// FriendshipEvent extracted from Friendship.cs (was Friendship.Event).
// Do not redefine these elsewhere.

namespace PokemonGame.Enums.PokemonEnum
{
    // Which ability slot the Pokemon has (used by PartyPokemon, BoxPokemon)
    public enum AbilityType : byte
    {
        Ability1,
        Ability2,
        AbilityH,
        NonStandard
    }

    // Events that change friendship/happiness value (used by Friendship helper)
    public enum FriendshipEvent : byte
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
}
