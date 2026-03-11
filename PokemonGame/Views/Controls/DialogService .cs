using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PokemonGame.Views.Windows;

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


        public Task ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return Task.CompletedTask;
        }


        public Task ShowSuccess(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }


        public Task<string> ShowInput(string title, string message, string defaultValue = "")
        {
            // Ensure this code runs on the UI thread
            string result = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                // InputDialog is a WPF Window defined in your Views layer
                var dialog = new InputDialog(title, message, defaultValue);
                if (dialog.ShowDialog() == true)
                    result = dialog.ResponseText;
            });

            return Task.FromResult(result);
        }
        public Task<string> ShowSelection(string title, string message, IEnumerable<string> options)
        {
            string result = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Create a simple selection dialog
                var dialog = new SelectionDialog(title, message, options);
                if (dialog.ShowDialog() == true)
                    result = dialog.SelectedOption;
            });

            return Task.FromResult(result);
        }
    }
}
