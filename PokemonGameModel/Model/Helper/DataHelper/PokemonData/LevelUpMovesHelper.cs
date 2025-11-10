using PokemonGame.Core.Config;
using PokemonGame.Services.Data.Pokemon;
using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Core.Model.Helper.DataHelper.PokemonData
{
    public static class LevelUpMovesHelper
    {
        public static bool CanLearnMoveEventually(List<LevelUpMovesData> moves,LevelUpMovesData move)
        {
            if(moves.Contains(move))
            {
                return true;
            }
            return false;
        }
        public static List<LevelUpMovesData> GetAllLearnableMoves(List<LevelUpMovesData> moves,byte level)
        {
            List<LevelUpMovesData> learnableMoves = new List<LevelUpMovesData>();
            foreach(var move in moves)
            {
                if(move.Level <= level)
                {
                    learnableMoves.Add(move);
                }
            }
            return learnableMoves;
        }
        public static LevelUpMovesData? GetMoveAtLevel(List<LevelUpMovesData> moves,byte level)
        {
            foreach(var move in moves)
            {
                if(move.Level == level)
                {
                    return move;
                }
            }
            return null;
        }
        public static LevelUpMovesData? GetNextMove(List<LevelUpMovesData> moves,byte level)
        {
            LevelUpMovesData? nextMove = null;
            foreach(var move in moves)
            {
                if(move.Level > level)
                {
                    if(nextMove == null || move.Level < nextMove.Level)
                    {
                        nextMove = move;
                    }
                }
            }
            return nextMove;
        }
        public static MoveNameType[] GetAllMoveNames(List<LevelUpMovesData> moves)
        {
            List<MoveNameType> moveNames = new List<MoveNameType>();
            foreach(var move in moves)
            {
                moveNames.Add(move.MoveName);
            }
            return moveNames.ToArray();
        }
        public static MoveNameType[] GetDefaultMoves(List<LevelUpMovesData> moves, byte level)
        {
            return moves
                .FindAll(m => m.Level <= level)
                .Select(m => m.MoveName)                                                 
                .Distinct()
                .Reverse()                                                                
                .Take(PokemonConstants.NumMoves)
                .ToArray();
        }
    }
}
