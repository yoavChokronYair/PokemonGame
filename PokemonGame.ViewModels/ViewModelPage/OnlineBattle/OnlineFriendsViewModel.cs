using System.Collections.ObjectModel;
using System.Windows;
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

        public OnlineFriendsViewModel(UserStore user, IDialogService dialogService)
        {
            _user = user;
            _dialogService = dialogService;
            _friendService = new FriendService();
            AddFriendCommand = new AsyncRelayCommand(OnAddFriendAsync);
            LoadDummyFriends();
        }
        // In OnlineFriendsViewModel
        public void Cleanup()
        {
            foreach (var friend in Friends)
                friend.Cleanup();
        }
        private void LoadDummyFriends()
        {
            Friends.Clear();

            var dummyFriends = new[]
            {
                new FriendItemViewModel("Ash",    isOnline: true,  _friendService, level: 42),
                new FriendItemViewModel("Misty",  isOnline: true,  _friendService, level: 38),
                new FriendItemViewModel("Brock",  isOnline: false, _friendService, level: 55),
                new FriendItemViewModel("Gary",   isOnline: false, _friendService, level: 61),
                new FriendItemViewModel("Tracey", isOnline: true,  _friendService, level: 27),
                new FriendItemViewModel("May",    isOnline: false, _friendService, level: 33),
            };

            foreach (var vm in dummyFriends)
            {
                vm.OnRequestRemove += OnFriendRemoved;
                Friends.Add(vm);
            }
        }

        // TODO: swap LoadDummyFriends() with this when ready
        private void LoadFriends()
        {
            Friends.Clear();
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
            string? friendIdentifier = await _dialogService.ShowInputAsync("Add Friend", "Enter Friend's Player ID:");

            if (string.IsNullOrWhiteSpace(friendIdentifier) || !int.TryParse(friendIdentifier, out int friendID))
                return;

            bool success = _friendService.SendRequest(_user.BattlePlayerID, friendID);

            if (success)
                await _dialogService.ShowSuccessAsync("Success", "Friend request sent!");
            else
                await _dialogService.ShowErrorAsync("Error", "Could not send request. Check the ID or friendship status.");

            LoadFriends();
        }

        private void OnFriendRemoved(FriendItemViewModel friendVm)
        {
            Friends.Remove(friendVm);
        }
    }

    public class FriendItemViewModel : ViewModelBase
    {
        private readonly BattlePlayerFriendData? _data;
        private readonly FriendService _friendService;
        public event Action<FriendItemViewModel>? OnRequestRemove;

        public string Username { get; }
        public bool IsOnline { get; }
        public int Level { get; }

        private bool _isPopupOpen;
        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set => SetProperty(ref _isPopupOpen, value);
        }

        public IRelayCommand InviteCommand { get; }
        public IRelayCommand RemoveCommand { get; }
        public IRelayCommand TogglePopupCommand { get; }
        public IRelayCommand ClosePopupCommand { get; }

        // ── Dummy constructor ──────────────────────────────────
        public FriendItemViewModel(string username, bool isOnline, FriendService friendService, int level = 1)
        {
            Username = username;
            IsOnline = isOnline;
            Level = level;
            _friendService = friendService;

            InviteCommand = new RelayCommand(OnInvite, () => IsOnline);
            RemoveCommand = new RelayCommand(OnRemove);
            TogglePopupCommand = new RelayCommand(() => IsPopupOpen = true);
            ClosePopupCommand = new RelayCommand(() => IsPopupOpen = false);

            Application.Current.MainWindow.PreviewMouseDown += OnWindowMouseDown;
        }

        // ── Real constructor ───────────────────────────────────
        public FriendItemViewModel(BattlePlayerFriendData data, FriendService friendService)
        {
            _data = data;
            _friendService = friendService;
            Username = data.FriendPlayerID.ToString();
            IsOnline = data.Status == "Online";
            Level = data.Level;

            InviteCommand = new RelayCommand(OnInvite, () => IsOnline);
            RemoveCommand = new RelayCommand(OnRemove);
            TogglePopupCommand = new RelayCommand(() => IsPopupOpen = true);
            ClosePopupCommand = new RelayCommand(() => IsPopupOpen = false);

            Application.Current.MainWindow.PreviewMouseDown += OnWindowMouseDown;
        }
        private void OnWindowMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (IsPopupOpen)
                IsPopupOpen = false;
        }
        public void Cleanup()
        {
            Application.Current.MainWindow.PreviewMouseDown -= OnWindowMouseDown;
        }
        private void OnRemove()
        {
            if (_data != null)
                _friendService.RemoveFriendship(_data.PlayerID, _data.FriendPlayerID);

            // ✅ Unsubscribe to avoid memory leak when friend is removed
            Application.Current.MainWindow.PreviewMouseDown -= OnWindowMouseDown;
            IsPopupOpen = false;
            OnRequestRemove?.Invoke(this);
        }
        private void OnInvite() { /* TODO */ }

        
    }
}