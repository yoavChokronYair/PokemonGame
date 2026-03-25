using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class SideMenuViewModel : ViewModelBase
    {
        private readonly NavigationStore _contentNavigationStore;
        private readonly NavigationStore _rootNavigationStore;

        private bool _isMenuOpen = true;
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set
            {
                _isMenuOpen = value;
                OnPropertyChanged(nameof(IsMenuOpen));
                OnPropertyChanged(nameof(MenuWidth));
                OnPropertyChanged(nameof(TextVisibility));
            }
        }

        // Collapsed = only icon width, expanded = full width
        public int MenuWidth => IsMenuOpen ? 200 : 50;

        // Hide button labels when collapsed
        public string TextVisibility => IsMenuOpen ? "Visible" : "Collapsed";

        public ICommand ToggleMenuCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand HistoryCommand { get; }
        public ICommand FriendsCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand TeamCommand { get; }
        public ICommand ExitCommand { get; }

        public SideMenuViewModel(
            NavigationStore contentNavigationStore,
            NavigationStore rootNavigationStore,   
            Func<BattleMenuViewModel> createHome,
            Func<HistoryBattleViewModel> createHistory,
            Func<OnlineFriendsViewModel> createFriends,
            Func<TeamBuilderViewModel> createTeam,
            Func<ProfileViewModel> createProfile,
            Func<ViewModelBase>? exit = null)
        {
            _contentNavigationStore = contentNavigationStore;
            _rootNavigationStore = rootNavigationStore;
            ToggleMenuCommand = new RelayCommand(() => IsMenuOpen = !IsMenuOpen);

            HomeCommand = new NavigateCommand(_contentNavigationStore, createHome);

            HistoryCommand = new NavigateCommand(_contentNavigationStore, createHistory);

            FriendsCommand = new NavigateCommand(_contentNavigationStore, createFriends);

            TeamCommand = new NavigateCommand(_contentNavigationStore, createTeam);

            ProfileCommand = new NavigateCommand(_contentNavigationStore, createProfile);

            if (exit != null)
                ExitCommand = new NavigateCommand(_rootNavigationStore, exit);
        }
    }
}