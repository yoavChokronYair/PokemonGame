using PokemonGame.Enums;
using PokemonGame.Model.Data;
using PokemonGame.Model.PokemonCreation;
using System.ComponentModel;

namespace PokemonGame.ViewModel
{
    public class WildPokemonBattleViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public PokemonData Rival  { get; set; }
        public PokemonData Team { get; set; }
        public WildPokemonBattleViewModel(PokemonData Rival, PokemonData Team, WildPokemonGenartion TeamEncounter, WildPokemonGenartion RivalEncounter)
        {
            this.Rival = Rival;
            this.Team = Team;
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

