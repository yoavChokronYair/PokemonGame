namespace PokemonGame.Model.Domain.Pokemon
{
    public class BaseStats
    {
        public int HP { get; }
        public int Attack { get; }
        public int Defense { get; }
        public int SpecialAttack { get; }
        public int SpecialDefense { get; }
        public int Speed { get; }

        public BaseStats(int hp, int attack, int defense,
                         int specialAttack, int specialDefense, int speed)
        {
            HP = hp;
            Attack = attack;
            Defense = defense;
            SpecialAttack = specialAttack;
            SpecialDefense = specialDefense;
            Speed = speed;
        }
        public BaseStats()
        {
            HP = 1;
            Attack = 1;
            Defense = 1;
            SpecialAttack = 1;
            SpecialDefense = 1;
            Speed = 1;
        }
    }
}
