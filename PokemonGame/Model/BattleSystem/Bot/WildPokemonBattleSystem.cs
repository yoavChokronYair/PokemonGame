using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Model.BattleSystem.Bot
{
    public class WildPokemonBot : IBotBattle
    {
        private EnemyPokemonGeneration _wildPokemon;
        private List<bool> _isFainted;

        public WildPokemonBot(EnemyPokemonGeneration wildPokemon)
        {
            _wildPokemon = wildPokemon;
            RivalTeam = new List<EnemyPokemonGeneration> { _wildPokemon };
            _isFainted = new List<bool> { false };
        }

        // -----------------------------
        // IBotBattle Implementation
        // -----------------------------

        public void updateData()
        {
            _isFainted[0] = _wildPokemon.CurrentHp <= 0;
        }
        public int ActivePokemonHp => _wildPokemon.CurrentHp;
        public List<EnemyPokemonGeneration> RivalTeam { get; }
        public EnemyPokemonGeneration ActivePokemon => _wildPokemon;
        public List<bool> IsFainted => _isFainted;
        public void ChooseNextPokemon()
        {
            // Wild Pokémon can't switch - stays as-is
        }
        public MoveData ChooseMove()
        {
            var availableMoves = _wildPokemon.Moves.Where(m => m.Key.PP > 0).ToList();

            if (availableMoves.Count == 0)
                return null;

            // Simple AI: choose a random valid move
            Random rand = new Random();
            int index = rand.Next(availableMoves.Count);
            availableMoves[index].Key.PP--;

            return availableMoves[index].Key;
        }
        public void OnBattleEnd(bool won)
        {
            // No special behavior for wild Pokémon after battle
        }
        public void HealPokemon(string item)
        {
            // Wild Pokémon cannot heal
        }
        public void SwitchPokemon()
        {
            // Wild Pokémon never switches
           
        }
        public bool ShouldSwitchPokemon(PlayerPokemonGeneration playerPokemon)
        {
            return false; // Wild Pokémon never switches
        }
        public bool HasProirerty(PlayerPokemonGeneration playerPokemon)
        {
            // Determines if wild Pokémon goes first based on Speed
            return _wildPokemon.IVs.Speed >= playerPokemon.IVs.Speed;
        }
        public void ReceiveDamage(PlayerPokemonGeneration playerPokemon, MoveData move)
        {
            Console.WriteLine($"{ActivePokemon.Nickname} used {move.ename}!");
            ActivePokemon.CurrentHp -= BattleCalculator.ExecuteMove(ActivePokemon, playerPokemon, move).Damage; // Simple damage logic
        }
    }
}
