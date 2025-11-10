using PokemonGame.Core.Model.Helper.BattleHelper;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.Services.Data;

namespace PokemonGame.Interface
{
    public interface IBotBattle
    {
        int activePokemonHp{ get; set; }
        EnemyPokemonGeneration activePokemon { get; set; }
        int UpdateData(PlayerPokemonGeneration playerPokemon, IMoveResult moveResult,int currentHp);
      //  bool HasPriority(MoveData rivalMove, IMoveResult playerMove);
        void ChooseNextPokemon(); // Called when a Pokémon faints
        bool ShouldSwitchPokemon();
        void SwitchPokemon();
        //MoveData ChooseMove(); // Called to select a move during battle
        int HealPokemon(string item);
        MoveResult ExecuteMove();
        void ReceiveDamage();
        int EndTurn(bool HasPriority);
    }
}
