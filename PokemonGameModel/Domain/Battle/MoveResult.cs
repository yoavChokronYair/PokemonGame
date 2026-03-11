// Design: Data Transfer Object — carries the result of one move execution back to the caller.
// Layer: Domain/Battle — implements IMoveResult; used by BattleCalculatorHelper and bot classes.
// Extracted from BattleCaculaterHelper.cs where it was incorrectly colocated with the calculator.

using PokemonGame.Enums;
using PokemonGame.Interface;

namespace PokemonGame.Model.Domain.Battle
{
    public class MoveResult : IMoveResult
    {
        public int Damage { get; set; }
        public bool IsSwitch { get; set; }
        public StatusType StatusEffect { get; set; }
        public int Priority { get; set; }

        public MoveResult()
        {
            Damage = 0;
            IsSwitch = false;
            StatusEffect = StatusType.None;
            Priority = 0;
        }
    }
}
