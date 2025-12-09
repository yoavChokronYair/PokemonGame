using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows.Navigation;

namespace PokemonGame.ViewModels.SignUp
{
    public class GameModeChooserViewModel : ObservableObject
    {
        private readonly IOnlineProfileService _onlineProfileService;
        private readonly IDialogService _dialogService;

        public ICommand StoryModeCommand { get; }
        public ICommand OnlineModeCommand { get; }

        public GameModeChooserViewModel(
            IOnlineProfileService onlineProfileService,
            IDialogService dialogService)
        {
            _onlineProfileService = onlineProfileService;
            _dialogService = dialogService;

            StoryModeCommand = new RelayCommand(OnStoryMode);
            OnlineModeCommand = new AsyncRelayCommand(OnOnlineModeAsync);
        }

        private void OnStoryMode()
        {
            
        }

        private async Task OnOnlineModeAsync()
        {
            var profile = await _onlineProfileService.GetProfileAsync();

            if (profile == null)
            {
                bool create = await _dialogService.ShowConfirm(
                    "Online Profile",
                    "You don't have an online profile. Create one?"
                );

                if (create)
                {
                    bool success = await _onlineProfileService.CreateProfileAsync();

                    if (success)
                    {
                        await _dialogService.ShowMessage("Profile created!");
                    }
                    return;
                }
            }
            else
            {
            }
        }
    }
}
