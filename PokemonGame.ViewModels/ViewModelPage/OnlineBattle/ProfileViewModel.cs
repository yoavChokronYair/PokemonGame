using PokemonGame.ViewModels.ViewModelHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class ProfileViewModel : ViewModelBase
    {
        private string displayName;
        public string DisplayName
        {
            get => displayName;
            set
            {
                if (displayName != value)
                {
                    displayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        private string userName;
        public string UserName
        {
            get => userName;
            set
            {
                if (userName != value)
                {
                    userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        private bool isDarkMode;
        public bool IsDarkMode
        {
            get => isDarkMode;
            set
            {
                if (isDarkMode != value)
                {
                    isDarkMode = value;
                    OnPropertyChanged(nameof(IsDarkMode));
                }
            }
        }
    }

}
