namespace PokemonGame.Interface
{
    public interface IStatValues
    {
        int HP { get; set; }
        int Attack { get; set; }
        int Defense { get; set; }
        int SpecialAttack { get; set; }
        int SpecialDefense { get; set; }
        int Speed { get; set; }
    }
}
