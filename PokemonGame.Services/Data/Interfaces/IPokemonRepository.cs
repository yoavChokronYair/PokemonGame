using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Data.Interfaces
{
    internal interface IPokemonRepository
    {
        abstract PokemonData LoadPokemonData(int pokemonID);
        List<PokemonData> GetAllPokemon();

        PokemonFormData LoadFormData(int pokemonID);
        List<PokemonFormData> GetAllFormData();

        BaseStatsData LoadBaseStatsData(int pokemonID);
        List<BaseStatsData> GetAllBaseStats();

        EvolutionData LoadEvolutionData(int pokemonID);
        List<EvolutionData> GetAllEvolution();

        EggMoveData LoadEggMovesData(int pokemonID);
        List<EggMoveData> GetAllEggMoves();

        LevelUpMoveData LoadLevelUpMovesData(int pokemonID);
        List<LevelUpMoveData> GetAllLevelUpMoves();
    }

}
