// Move classification enums extracted from MoveDomain.cs.
// Do not redefine these in any other file.
// Used by: MoveDomain, CreateMoves, BattleCalculatorHelper

namespace PokemonGame.Enums.MovesEnum
{
    public enum MoveCategory { Physical, Special, Status }
    public enum MoveTarget { Opponent, Self, Both, AllOpponents, AllAllies }
}
