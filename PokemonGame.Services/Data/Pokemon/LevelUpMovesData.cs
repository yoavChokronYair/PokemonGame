using PokemonGame.Services.Enums.PokemonEnum;

namespace PokemonGame.Services.Data.Pokemon
{
    public sealed class LevelUpMovesData
    {
        private MoveNameType moveName;
        private byte level;
        private int pokemonID;

        public MoveNameType MoveName { get => moveName; set => moveName = value; }
        public byte Level { get => level; set => level = value; }
        public int PokemonID { get => pokemonID; set => pokemonID = value; }
    }
}
