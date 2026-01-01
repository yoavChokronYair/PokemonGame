using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData.User.OnlinePlayer;
using PokemonGame.ViewModels.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class OnlineFriendsViewModel : ViewModelBase
    {
        public ObservableCollection<OnlineFriend> Friends { get; }

        public ICommand AddFriendCommand { get; }

        public OnlineFriendsViewModel()
        {
            Friends = new ObservableCollection<OnlineFriend>
        {
            new OnlineFriend { Username = "Ash", IsOnline = true },
            new OnlineFriend { Username = "Misty", IsOnline = false },
            new OnlineFriend { Username = "Brock", IsOnline = true },
            new OnlineFriend { Username = "Gary", IsOnline = false }
        };

            AddFriendCommand = new RelayCommand(AddFriend);
        }

        private void AddFriend()
        {
            // later: open dialog / send request
        }
    }
}
