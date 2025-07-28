using PokemonGameModel.Model.Data;
using PokemonGameModel.Model.Helper;
using PokemonGameModel.Model.PokemonCreation;

namespace PokemonGameModel.Interface
{
    public interface IBotBattle
    {
        int _ActivePokemonHp{ get; set; }
        EnemyPokemonGeneration _ActivePokemon { get; set; }
        int UpdateData(PlayerPokemonGeneration playerPokemon, IMoveResult moveResult,int currentHp);
        bool HasProirerty();
        void ChooseNextPokemon(); // Called when a Pokémon faints
        bool ShouldSwitchPokemon();
        void SwitchPokemon();
        MoveData ChooseMove(); // Called to select a move during battle
        int HealPokemon(string item);
        MoveResult ExecuteMove();
        void ReceiveDamage();
        int EndTurn();
    }
}
