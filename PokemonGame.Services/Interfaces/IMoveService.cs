using PokemonGame.Services.Data.GameData.Move;

namespace PokemonGame.Services.Interfaces
{
    public interface IMoveService
    {
        MoveTree? GetMove(string name);
    }
    public class MoveTree
    {
        public MoveData Move { get; set; } = null!;
        public int Priority { get; set; }
        public int CritStage { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<MoveDecorator> Decorators { get; set; } = new();
        public List<MoveAttempt> Attempts { get; set; } = new();
    }
}
