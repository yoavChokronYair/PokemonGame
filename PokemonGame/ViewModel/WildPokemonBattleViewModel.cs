using CommunityToolkit.Mvvm.Input;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.BattleSystem.Bot;
using PokemonGame.Model.BattleSystem.Player;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel.BattleMenu;
using PokemonGame.ViewModel.ViewModelHelper;
using PokemonGame.Views.UserControls.PokemonBattle;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PokemonGame.ViewModel
{
    public class MoveResults : IMoveResult
    {
        public MoveResults(int damage,bool isSwitch,StatusType statusType)
        {
            Damage = damage;
            IsSwitch = isSwitch;
            StatusEffect = statusType;
        }

        public int Damage { get; set; }
        public bool IsSwitch { get; set; }
        public StatusType StatusEffect { get; set; }// You can expand this for status names
    }
    public class WildPokemonBattleViewModel : ViewModelBase
    {
        // Existing battle properties
        private readonly NavigationStore _NavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;
        public ICommand KeyPressedCommand { get; }

        public WildPokemonBattleViewModel(PlayerPokemonBot team, WildPokemonBot rival,NavigationStore navigationStore)
        {
            MoveList = new ObservableCollection<MoveViewModel>(team._ActivePokemon.Moves
                .Select(kvp => new MoveViewModel
                {
                    Name = kvp.Key.ename.Replace(">", ""), // Remove all '>' characters
                    MaxPP = kvp.Value,
                    CurrentPP = kvp.Value,
                    Type = kvp.Key.Type.ToString()
                }));
            _NavigationStore = navigationStore;
            _NavigationStore.CurrentViewModel = new PokemonBattleMenuViewModel(navigationStore,this);
            Team = team;
            Rival = rival;
            Gender = team._ActivePokemon.IsMale ? "♂" : "♀";
            TeamCurrentHp = team._ActivePokemonHp;
            RivalCurrentHp = rival._ActivePokemonHp;
            _NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
            

        }

        public void MakeMove(string move)
        {
            PlayerPokemonGeneration player = Team._ActivePokemon;
            EnemyPokemonGeneration enemy = Rival._ActivePokemon;
            MoveData moveData = player.Moves.Keys.FirstOrDefault(m => m.ename == move);
            MoveResult moveResult = Team.ExecuteMove(moveData);
            MoveResults teamMove = new MoveResults(moveResult.Damage,moveResult.IsSwitch,moveResult.StatusEffect);
            MoveResult  RivalMove = Rival.ExecuteMove();
            RivalCurrentHp = Rival.UpdateData(player, teamMove, (int)rivalCurrentHp);
            TeamCurrentHp = Team.UpdateData(enemy,RivalMove,(int)teamCurrentHp);
            RivalCurrentHp = Rival.EndTurn();
            TeamCurrentHp = Team.EndTurn();
        }
        private void OnCurrentViewModelChanged()
        {
            
            OnPropertyChanged(nameof(CurrentViewModel));
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

        private ObservableCollection<MoveViewModel> moveList;
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
