using PokemonGameModel.Model.PokemonCreation;
using PokemonGameModel.Enums;


namespace PokemonGameModel.Model.Manager
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
        public List<PlayerPokemonGeneration> _playerPokemons;
        public PlayerPokemonGeneration[] _playerPokemonTeam = new PlayerPokemonGeneration[6];

        // === Add Pokémon ===
        public void AddPokemonToBox(PlayerPokemonGeneration pokemon)
        {
            _playerPokemons.Add(pokemon);
        }
        public void AddPokemonToTeam(PlayerPokemonGeneration pokemon,int index)
        {
            _playerPokemonTeam[index] = pokemon;
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
            return _playerPokemonTeam.FirstOrDefault(p => !p.IsFainted);
        }

        // === Get All Healthy Pokémon ===
        public List<PlayerPokemonGeneration> GetAvailableForBattle()
        {
            return _playerPokemons.Where(p => !p.IsFainted).ToList();
        }
        public void AddPokemonToPartyAfterCatching(PlayerPokemonGeneration pokemon)
        {
            int currentCount = _playerPokemonTeam.Count(p => p != null);

            if (currentCount<= 6) { 
            
                _playerPokemonTeam[currentCount-1] = pokemon;
            };
        }
        // === Heal All ===
        public void HealAll()
        {
            foreach (var poke in _playerPokemonTeam)
            {
                poke.CurrentHp = poke.MaxHP;
                poke.StatusType = StatusType.None;
                poke.IsFainted = false;
            }
        }

        // === Party Wipe Check ===
        public bool AreAllFainted()
        {
            return _playerPokemonTeam.All(p => p.IsFainted);
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
