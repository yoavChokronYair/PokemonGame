using PokemonGame.Model.Data;
using PokemonGame.Model.PokemonCreation;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PokemonGame.ViewModel
{
    public class WildPokemonBattleViewModel : INotifyPropertyChanged
    {
        // Constructor
        public WildPokemonBattleViewModel(PlayerPokemonGeneration team, EnemyPokemonGeneration rival)
        {
            this.Team = team;
            this.Rival = rival;
            this.Gender = rival.IsMale ? "♂" : "♀";
            this.TeamCurrentHp = team.CurrentHp;
            this.RivalCurrentHp = rival.CurrentHp;
            this.Moves = team.Moves.Keys.ToList();
            this.Type = "None";
        }

        // Properties

        private PlayerPokemonGeneration team;
        public PlayerPokemonGeneration Team
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

        private EnemyPokemonGeneration rival;
        public EnemyPokemonGeneration Rival
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
        private List<MoveData> moves;
        public List<MoveData> Moves
        {
            get => moves;
            set
            {
                if (moves != value)
                {
                    moves = value;
                    OnPropertyChanged(nameof(Moves));
                }
            }
        }
        private int currentPP;
        public int CurrentPP
        {
            get => currentPP;
            set
            {
                if (currentPP != value)
                {
                    currentPP = value;
                    OnPropertyChanged(nameof(CurrentPP));
                }
            }
        }
        private int maxPP;
        public int MaxPP
        {
            get => maxPP;
            set
            {
                if (maxPP != value)
                {
                    maxPP = value;
                    OnPropertyChanged(nameof(maxPP));
                }
            }
        }
        private string type;
        public string Type
        {
            get => type;
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged(nameof(type));
                }
            }
        }
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
