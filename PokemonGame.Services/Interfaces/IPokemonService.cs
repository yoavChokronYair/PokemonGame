using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.GameData.PokemonData;

namespace PokemonGame.Services.Interfaces
{
    public interface IPokemonService
    {
        PokemonLoadResult? GetPokemon(int pokemonId);
        PokemonLoadResult GetPokemonFromInstance(BattlerPokemon battler);
        List<PokemonLoadResult> LoadTeamResults(int battlePlayerId);
        List<PokemonLoadResult> GenerateRandomTeam(int count = 6, int level = 50);
    }
    public class PokemonLoadResult
    {
        public BattlerPokemon Battler { get; set; } = null!;
        public PokemonGeneral General { get; set; } = null!;
        public PokemonStatsData Stats { get; set; } = null!;

        public List<string> MoveNames { get; set; } = new();
    }
}
