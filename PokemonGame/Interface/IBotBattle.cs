using PokemonGame.Enums;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;

namespace PokemonGame.Interface
{
    public interface IBotBattle
    {
        int _ActivePokemonHp{ get; set; }
        EnemyPokemonGeneration _ActivePokemon { get; set; }
        int UpdateData(PlayerPokemonGeneration playerPokemon, IMoveResult moveResult,int currentHp);
        bool HasProirerty();
        void ChooseNextPokemon();          // Called when a Pokémon faints
        bool ShouldSwitchPokemon();
        void SwitchPokemon();
        MoveData ChooseMove(); // Called to select a move during battle
        int HealPokemon(string item);
        MoveResult ExecuteMove();
        void ReceiveDamage();
        int EndTurn();
    }
}
