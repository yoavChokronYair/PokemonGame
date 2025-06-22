using PokemonGame.Model.BattleSystem.Bot;
using PokemonGame.Model.BattleSystem.Player;
using PokemonGame.Model.Data;
using PokemonGame.Model.PokemonCreation;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace PokemonGame.ViewModel
{
    public class WildPokemonBattleViewModel :ViewModelBase
    {
        public WildPokemonBattleViewModel(PlayerPokemonBot team, WildPokemonBot rival)
        {
            Team = team;
            Rival = rival;
            Gender = team._ActivePokemon.IsMale ? "♂" : "♀";
            TeamCurrentHp = team._ActivePokemonHp;
            RivalCurrentHp = rival._ActivePokemonHp;
            MoveList = new ObservableCollection<MoveViewModel>(team._ActivePokemon.Moves
            .Select(kvp => new MoveViewModel
            {
                Name = kvp.Key.ename,
                MaxPP = kvp.Value,
                CurrentPP = kvp.Value,
                Type = kvp.Key.Type.ToString()
            }));
        }
        private PlayerPokemonBot team;
        public PlayerPokemonBot Team
        {
            get => team;
            set
            {
                if (team != value)
                {
                    team = value;
                    OnPropertyChanged(nameof(Team));
                }
            }
        }
        private WildPokemonBot rival;
        public WildPokemonBot Rival
        {
            get => rival;
            set
            {
                if (rival != value)
                {
                    rival = value;
                    OnPropertyChanged(nameof(Rival));
                }
            }
        }
        private string gender;
        public string Gender
        {
            get => gender;
            set
            {
                if (gender != value)
                {
                    gender = value;
                    OnPropertyChanged(nameof(Gender));
                }
            }
        }

        private double teamCurrentHp;
        public double TeamCurrentHp
        {
            get => teamCurrentHp;
            set
            {
                if (teamCurrentHp != value)
                {
                    teamCurrentHp = value;
                    OnPropertyChanged(nameof(TeamCurrentHp));
                }
            }
        }

        private double rivalCurrentHp;
        public double RivalCurrentHp
        {
            get => rivalCurrentHp;
            set
            {
                if (rivalCurrentHp != value)
                {
                    rivalCurrentHp = value;
                    OnPropertyChanged(nameof(RivalCurrentHp));
                }
            }
        }

        private ObservableCollection<MoveViewModel> moveList { get; set; }
        public ObservableCollection<MoveViewModel> MoveList
        {
            get => moveList;
            set
            {
                if (moveList != value)
                {
                    moveList = value;
                    OnPropertyChanged(nameof(MoveList));
                
                }
            }
        }
    }
}
