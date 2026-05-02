using System;
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

        // --- NEW PROPERTY FOR DYNAMIC TITLE ---
        private string _currentPageTitle = "HOME";
        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            set
            {
                _currentPageTitle = value;
                OnPropertyChanged(nameof(CurrentPageTitle));
            }
        }

        public int MenuWidth => IsMenuOpen ? 200 : 50;
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
            Func<OnlineBattleMenuViewModel> createHome,
            Func<HistoryBattleViewModel> createHistory,
            Func<TeamBuilderViewModel> createTeam,
            Func<ProfileViewModel> createProfile,
            Func<ViewModelBase>? exit = null)
        {
            _contentNavigationStore = contentNavigationStore;
            _rootNavigationStore = rootNavigationStore;

            ToggleMenuCommand = new RelayCommand(() => IsMenuOpen = !IsMenuOpen);

            // Execute the navigation AND update the title
            HomeCommand = new RelayCommand(() => {
                new NavigateCommand(_contentNavigationStore, createHome).Execute(null);
                CurrentPageTitle = "HOME";
            });

            HistoryCommand = new RelayCommand(() => {
                new NavigateCommand(_contentNavigationStore, createHistory).Execute(null);
                CurrentPageTitle = "HISTORY";
            });

            TeamCommand = new RelayCommand(() => {
                new NavigateCommand(_contentNavigationStore, createTeam).Execute(null);
                CurrentPageTitle = "TEAM";
            });

            ProfileCommand = new RelayCommand(() => {
                new NavigateCommand(_contentNavigationStore, createProfile).Execute(null);
                CurrentPageTitle = "PROFILE";
            });

            if (exit != null)
            {
                ExitCommand = new RelayCommand(() => {
                    new NavigateCommand(_rootNavigationStore, exit).Execute(null);
                });
            }
        }
    }
}