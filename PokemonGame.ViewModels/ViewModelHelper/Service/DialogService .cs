using PokemonGame.ViewModels.SignUp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace PokemonGame.ViewModels.ViewModelHelper.Service
{
    public class DialogService : IDialogService
    {
        public Task<bool> ShowConfirm(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            return Task.FromResult(result == MessageBoxResult.Yes);
        }

        public Task ShowMessage(string message)
        {
            MessageBox.Show(message);
            return Task.CompletedTask;
        }
    }
}
