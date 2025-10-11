using PokemonGame.ViewModel.Map;
using PokemonGame.Interface;
using PokemonGame.Model.BattleSystem.Bot;
using PokemonGame.Model.BattleSystem.Player;
using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.ViewModel.BattleMenu;
using PokemonGame.ViewModel.ViewModelHelper;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PokemonGame.Enums;
using PokemonGame.Services.Data;

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
        public StatusType StatusEffect { get; set; }
        public int Priority { get; set; }
        StatusType IMoveResult.StatusEffect { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
    public class WildPokemonBattleViewModel : ViewModelBase
    {
        // Existing battle properties
        private readonly NavigationStore _NavigationStore;
        public readonly NavigationStore _PageNavigationStore;
        public ViewModelBase CurrentViewModel => _NavigationStore.CurrentViewModel;
        public MapViewModel _mainWindow;
        public WildPokemonBattleViewModel(PlayerPokemonBot team, WildPokemonBot rival,NavigationStore navigation,MapViewModel mainWindow)
        {
            _mainWindow = mainWindow;
            ImagePathRival = rival.activePokemon.Image;
            ImagePathTeam = team._ActivePokemon.Image;
            _PageNavigationStore = navigation;
            MoveList = new ObservableCollection<MoveViewModel>(team._ActivePokemon.Moves
                .Select(kvp => new MoveViewModel
                {
                    BaseName = kvp.Key.ename.Replace(">", ""), // Remove all '>' characters
                    MaxPP = kvp.Value,
                    CurrentPP = kvp.Value,
                    Type = kvp.Key.Type.ToString()
                })); 
            _NavigationStore = new NavigationStore();
            _NavigationStore.CurrentViewModel = new PokemonBattleMenuViewModel(_NavigationStore,navigation,this);
            Team = team;
            Rival = rival;
            Gender = team._ActivePokemon.IsMale ? "♂" : "♀";
            TeamCurrentHp = team._ActivePokemonHp;
            RivalCurrentHp = rival.activePokemonHp;
            _NavigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;
        }
        public async Task MakeMove(string move)
        {
            PlayerPokemonGeneration player = Team._ActivePokemon;
            EnemyPokemonGeneration enemy = Rival.activePokemon;
            MoveData moveData = player.Moves.Keys.FirstOrDefault(m => m.ename == move);
            MoveResult moveResult = Team.ExecuteMove(moveData);
            MoveResults teamMove = new MoveResults(moveResult.Damage,moveResult.IsSwitch,moveResult.StatusEffect);
            MoveResult  RivalMove = Rival.ExecuteMove();
            RivalCurrentHp = Rival.UpdateData(player, teamMove, (int)rivalCurrentHp);
            TeamCurrentHp = Team.UpdateData(enemy,RivalMove,(int)teamCurrentHp);
            RivalCurrentHp = Rival.EndTurn(false);
            TeamCurrentHp = Team.EndTurn();
            if (RivalCurrentHp <= 0)
            {
                await Task.Delay(1000); // Adjust delay time as needed (milliseconds)

                _PageNavigationStore.CurrentViewModel = _mainWindow;
            }
            
        }
        private void OnCurrentViewModelChanged()
        {   
            OnPropertyChanged(nameof(CurrentViewModel));
        }
        //binding
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
        private string imagePathRival;
        public string ImagePathRival
        {
            get => imagePathRival;
            set
            {
                if (imagePathRival != value)
                {
                    imagePathRival = value;
                    OnPropertyChanged(nameof(ImagePathRival));
                }
            }
        }
        private string imagePathTeam;
        public string ImagePathTeam
        {
            get => imagePathTeam;
            set
            {
                if (imagePathTeam != value)
                {
                    imagePathTeam = value;
                    OnPropertyChanged(nameof(ImagePathTeam));
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
