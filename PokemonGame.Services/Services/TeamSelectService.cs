using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Repositories.SQLite;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class TeamSelectService
    {
        private readonly SQLitePokemonRepository _pokemonCache;

        public TeamSelectService()
        {
            _pokemonCache = ServiceFactory.Instance.PokemonRepository;
        }

        public List<PokemonData> GetAllPokemon()
        {
            return _pokemonCache.GetAllPokemon();
        }

        public BaseStatsData GetBaseStats(int pokemonID)
        {
            return _pokemonCache.LoadBaseStatsData(pokemonID);
        }
    }
}