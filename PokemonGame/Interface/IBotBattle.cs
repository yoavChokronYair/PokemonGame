
using PokemonGame.Model.Data;
using PokemonGame.Model.PokemonCreation;
using System.Collections.Generic;

namespace PokemonGame.Interface
{
    internal interface IBotBattle
    {
        void updateData();
        int ActivePokemonHp{ get; }
        List<EnemyPokemonGeneration> RivalTeam { get; }
        EnemyPokemonGeneration ActivePokemon { get; }
        List<bool> IsFainted { get; }
        void ChooseNextPokemon();          // Called when a Pokémon faints
        MoveData ChooseMove(); // Called to select a move during battle
        void OnBattleEnd(bool won);
        void HealPokemon(string item);
        void SwitchPokemon();
        bool ShouldSwitchPokemon(PlayerPokemonGeneration playerPokemon);
        bool HasProirerty(PlayerPokemonGeneration playerPokemonGenaration);
    }
}
