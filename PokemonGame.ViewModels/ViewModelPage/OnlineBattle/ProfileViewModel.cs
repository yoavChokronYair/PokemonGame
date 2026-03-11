using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Store;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class ProfileViewModel : ViewModelBase
    {
        private readonly ProfileService _handler;

        private string _displayName = string.Empty;
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName != value)
                {
                    _displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        private string _userName = string.Empty;
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    OnPropertyChanged(nameof(IsDarkMode));
                }
            }
        }

        public ProfileViewModel(UserStore userStore)
        {
            _handler = new ProfileService();
            LoadProfile(userStore.Username);
        }

        private void LoadProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return;

            var user = _handler.GetUser(username);
            if (user == null)
                return;

            UserName = user.UserName;
            DisplayName = user.UserName; // defaults to username; extend later with a DisplayName column
        }
    }
}