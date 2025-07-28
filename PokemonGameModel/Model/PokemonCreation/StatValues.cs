using PokemonGameModel.Interface;

namespace PokemonGameModel.Model.PokemonCreation
{
    public class StatValues:IStatValues
    {
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpecialAttack { get; set; }
        public int SpecialDefense { get; set; }
        public int Speed { get; set; }
    }
}
