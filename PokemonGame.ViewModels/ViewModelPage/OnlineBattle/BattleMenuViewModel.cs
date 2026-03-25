using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class BattleMenuViewModel : ViewModelBase
    {
        private bool _isOnline = true;
        public bool IsOnline
        {
            get => _isOnline;
            set { SetProperty(ref _isOnline, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        private bool _is1v1 = true;
        public bool Is1v1
        {
            get => _is1v1;
            set { SetProperty(ref _is1v1, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        private bool _isRandom = true;
        public bool IsRandom
        {
            get => _isRandom;
            set { SetProperty(ref _isRandom, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        private bool _isSetTeam;
        public bool IsSetTeam
        {
            get => _isSetTeam;
            set { SetProperty(ref _isSetTeam, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        private bool _isNoLegendaries;
        public bool IsNoLegendaries
        {
            get => _isNoLegendaries;
            set { SetProperty(ref _isNoLegendaries, value); OnPropertyChanged(nameof(SelectionSummary)); }
        }

        // ✅ Computed every time any radio changes
        public string SelectionSummary
        {
            get
            {
                string mode = IsOnline ? "Online" : "Offline";
                string format = Is1v1 ? "1v1" : "2v2";
                string gameMode = IsRandom ? "Random" : IsSetTeam ? "Set Team" : "No Legendaries";
                return $"{mode}  •  {format}  •  {gameMode}";
            }
        }

        public ICommand PlayCommand { get; }

        public BattleMenuViewModel()
        {
            PlayCommand = new RelayCommand(() =>
            {
                // hook up matchmaking / navigation here
            });
        }
    }
}