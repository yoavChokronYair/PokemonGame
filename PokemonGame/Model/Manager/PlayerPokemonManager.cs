using PokemonGame.Model.PokemonCreation;
using PokemonGame.Enums;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Model.Manager
{
    public class PlayerPokemonManager
    {
        // === Singleton Instance ===
        private static PlayerPokemonManager instance;
        public static PlayerPokemonManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new PlayerPokemonManager();
                return instance;
            }
        }

        // === Private Constructor ===
        private PlayerPokemonManager()
        {
            _playerPokemons = new List<PlayerPokemonGeneration>();
        }

        // === Internal Pokémon Collection ===
        private List<PlayerPokemonGeneration> _playerPokemons;
        private PlayerPokemonGeneration[] PlayerPokemonTeam = new PlayerPokemonGeneration[6];

        // === Add Pokémon ===
        public void AddPokemon(PlayerPokemonGeneration pokemon)
        {
            _playerPokemons.Add(pokemon);
        }

        // === Remove by ID ===
        public bool RemovePokemon(int id)
        {
            var poke = _playerPokemons.FirstOrDefault(p => p.ID == id);
            if (poke != null)
            {
                _playerPokemons.Remove(poke);
                return true;
            }
            return false;
        }

        // === Get by ID ===
        public PlayerPokemonGeneration GetPokemonById(int id)
        {
            return _playerPokemons.FirstOrDefault(p => p.ID == id);
        }

        // === Get First Healthy Pokémon ===
        public PlayerPokemonGeneration GetFirstAvailable()
        {
            return PlayerPokemonTeam.FirstOrDefault(p => !p.IsFainted);
        }

        // === Get All Healthy Pokémon ===
        public List<PlayerPokemonGeneration> GetAvailableForBattle()
        {
            return _playerPokemons.Where(p => !p.IsFainted).ToList();
        }
        public void AddPokemonToPartyAfterCatching(PlayerPokemonGeneration pokemon)
        {
            int currentCount = PlayerPokemonTeam.Count(p => p != null);

            if (currentCount<= 6) { 
            
                PlayerPokemonTeam[currentCount-1] = pokemon;
            };
        }
        // === Heal All ===
        public void HealAll()
        {
            foreach (var poke in PlayerPokemonTeam)
            {
                poke.CurrentHp = poke.MaxHP;
                poke.StatusType = StatusType.None;
                poke.IsFainted = false;
            }
        }

        // === Party Wipe Check ===
        public bool AreAllFainted()
        {
            return PlayerPokemonTeam.All(p => p.IsFainted);
        }

        // === Sort by Level ===
        public void SortByLevel()
        {
            _playerPokemons = _playerPokemons.OrderBy(p => p.Level).ToList();
        }

        // === Clear All Pokémon (e.g., for new game/reset) ===
        public void ClearAll()
        {
            _playerPokemons.Clear();
        }
    }
}
