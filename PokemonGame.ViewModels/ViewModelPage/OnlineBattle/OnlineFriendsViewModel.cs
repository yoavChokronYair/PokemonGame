using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData.User;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;
using PokemonGame.ViewModels.ViewModelHelper.Service;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class OnlineFriendsViewModel : ViewModelBase
    {
        private readonly FriendService _friendService;
        private readonly UserStore _user;
        private readonly IDialogService _dialogService;

        public ObservableCollection<FriendItemViewModel> Friends { get; } = new();
        public IAsyncRelayCommand AddFriendCommand { get; }

        public OnlineFriendsViewModel(UserStore user,IDialogService dialogService)
        {
            _user = user;
            _dialogService = dialogService;
            _friendService = new FriendService(); // Or inject via constructor
            AddFriendCommand = new AsyncRelayCommand(OnAddFriendAsync);
            LoadFriends();
        }

        private void LoadFriends()
        {
            Friends.Clear();
            // Use the service to fetch data for the current player
            var dataList = _friendService.GetActiveFriends(_user.BattlePlayerID);

            foreach (var data in dataList)
            {
                var vm = new FriendItemViewModel(data, _friendService);
                vm.OnRequestRemove += OnFriendRemoved;
                Friends.Add(vm);
            }
        }

        private async Task OnAddFriendAsync()
        {
            // 1. Get the username/ID via your DialogService
            string? friendIdentifier = await _dialogService.ShowInputAsync("Add Friend", "Enter Friend's Player ID:");

            if (string.IsNullOrWhiteSpace(friendIdentifier) || !int.TryParse(friendIdentifier, out int friendID))
                return;

            // 2. Call the service
            bool success = _friendService.SendRequest(_user.BattlePlayerID, friendID);

            // 3. Provide feedback
            if (success)
                await _dialogService.ShowSuccessAsync("Success", "Friend request sent!");
            else
                await _dialogService.ShowErrorAsync("Error", "Could not send request. Check the ID or friendship status.");

            // Refresh list in case a request was accepted or state changed
            LoadFriends();
        }

        private void OnFriendRemoved(FriendItemViewModel friendVm)
        {
            Friends.Remove(friendVm);
        }
    }

    // --- The Item ViewModel ---
    public class FriendItemViewModel : ViewModelBase
    {
        private readonly BattlePlayerFriendData _data;
        private readonly FriendService _friendService;
        public event Action<FriendItemViewModel>? OnRequestRemove;

        public string Username => _data.FriendPlayerID.ToString(); // Or use a lookup for the actual Name
        public bool IsOnline => _data.Status == "Online"; // Map your status logic

        public IRelayCommand InviteCommand { get; }
        public IRelayCommand RemoveCommand { get; }

        public FriendItemViewModel(BattlePlayerFriendData data, FriendService friendService)
        {
            _data = data;
            _friendService = friendService;

            InviteCommand = new RelayCommand(OnInvite, () => IsOnline);
            RemoveCommand = new RelayCommand(OnRemove);
        }

        private void OnInvite() { /* Implementation */ }

        private void OnRemove()
        {
            // Use the injected service, not the ServiceFactory
            _friendService.RemoveFriendship(_data.PlayerID, _data.FriendPlayerID);
            OnRequestRemove?.Invoke(this);
        }
    }
}
