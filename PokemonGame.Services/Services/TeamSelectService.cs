using PokemonGame.Services.Data.DataCache;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Factory;
using System.Collections.Generic;

namespace PokemonGame.Services.Handler
{
    public class TeamSelectService
    {
        private readonly PokemonCacheService _pokemonCache;

        public TeamSelectService()
        {
            _pokemonCache = ServiceFactory.Instance.PokemonCache;
        }

        public List<PokemonData> GetAllPokemon()
        {
            return _pokemonCache.GetAllPokemon();
        }

        public BaseStatsData GetBaseStats(int pokemonID)
        {
            return _pokemonCache.GetBaseStats(pokemonID);
        }
    }
}