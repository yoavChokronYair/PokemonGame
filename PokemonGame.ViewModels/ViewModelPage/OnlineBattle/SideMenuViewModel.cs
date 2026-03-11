using System.Windows.Input;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class SideMenuViewModel : ViewModelBase
    {
        private readonly NavigationStore _contentNavigationStore;

        public ICommand HistoryCommand { get; }
        public ICommand FriendsCommand { get; }
        public ICommand ProfileCommand { get; }
        public ICommand TeamCommand { get; }
        public ICommand ExitCommand { get; }

        public SideMenuViewModel(
            NavigationStore contentNavigationStore,
            Func<HistoryBattleViewModel> createHistory,
            Func<OnlineFriendsViewModel> createFriends,
            Func<TeamSelectPageViewModel> createTeam,
            Func<ProfileViewModel> createProfile,
            Func<ViewModelBase>? exit = null
        )
        {
            _contentNavigationStore = contentNavigationStore;

            HistoryCommand = new NavigateCommand(
                _contentNavigationStore,
                createHistory
            );

            FriendsCommand = new NavigateCommand(
                _contentNavigationStore,
                createFriends
            );

            TeamCommand = new NavigateCommand(
                _contentNavigationStore,
                createTeam
            );

            ProfileCommand = new NavigateCommand(
                _contentNavigationStore,
                createProfile
            );

            if (exit != null)
            {
                ExitCommand = new NavigateCommand(
                    _contentNavigationStore,
                    exit
                );
            }
        }
    }
}
