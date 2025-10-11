using System.Collections.Generic;
using System.Linq;
using PokemonGame.Enums;
using PokemonGame.Model.Manager;

namespace PokemonGame.Model.Data
{
    public class PokemonData
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public PokemonType Type1 { get; set; }
        public PokemonType Type2 { get; set; }
        public string Ability1 { get; set; }
        public string Ability2 { get; set; }
        public string HiddenAbility { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpAtk { get; set; }
        public int SpDef { get; set; }
        public int Speed { get; set; }
        public int CatchRate { get; set; }
        public int BaseFriendship { get; set; }
        public int BaseExp { get; set; }
        public string GrowthRate { get; set; }
        public double MaleGenderPercent { get; set; }
        public List<LevelUpMove> Moves { get; set; } = new List<LevelUpMove>();
        public List<EvolutionData> Evolution { get; set; } = new List<EvolutionData>();

    }
    public class LevelUpMove
    {
        public int Level { get; set; }
        public string Move { get; set; }
        public MoveData Moves
        {
            get
            {
                return GameDataManager.Instance.MoveData.Moves
                    .FirstOrDefault(m => m.ename == Move);
            }
        }
    }
    public class EvolutionData
    {
        public int Level { get; set; } // -1 if evolving via stone or other method
        public int Evolution { get; set; } // National Dex number of the evolved form
    }
    public class PokemonDataList
    {
        public List<PokemonData> Starters { get; set; } = new List<PokemonData>();
        public List<PokemonData> Pokemons { get; set; } = new List<PokemonData>();
        public List<PokemonData> AllPokemons
        {
            get
            {
                return Starters.Concat(Pokemons).ToList();
            }
        }
    }
}
