using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using PokemonGame.ViewModels.ViewModelPage.BattleMenu;
using PokemonGame.Views.Windows;

namespace PokemonGame.ViewModels.ViewModelHelper.Service
{
    public class DialogService : IDialogService
    {
        public Task<bool> ShowConfirmAsync(string title, string message)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo);
            return Task.FromResult(result == MessageBoxResult.Yes);
        }


        public Task ShowMessageAsync(string message)
        {
            MessageBox.Show(message);
            return Task.CompletedTask;
        }


        public Task ShowErrorAsync(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return Task.CompletedTask;
        }


        public Task ShowSuccessAsync(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }


        public Task<string> ShowInputAsync(string title, string message, string defaultValue = "")
        {
            // Ensure this code runs on the UI thread
            string result = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                // InputDialog is a WPF Window defined in your Views layer
                var dialog = new InputDialog(title, message, defaultValue);
                if (dialog.ShowDialog() == true)
                {
                    result = dialog.ResponseText;
                }
            });

            return Task.FromResult(result);
        }
        public Task<string> ShowSelectionAsync(string title, string message, IEnumerable<string> options)
        {
            string result = null;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Create a simple selection dialog
                var dialog = new SelectionDialog(title, message, options);
                if (dialog.ShowDialog() == true)
                {
                    result = dialog.SelectedOption;
                }
            });

            return Task.FromResult(result);
        }
        public Task<BattleResultAction> ShowBattleResultAsync(BattleViewModel vm)
        {
            BattleResultAction chosen = BattleResultAction.Back;

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Create the actual BattleResult WPF Window
                var window = new BattleResult
                {
                    DataContext = vm, // Set the current BattleViewModel as DataContext
                    Owner = Application.Current.MainWindow
                };

                // Close the window on the vm action request
                vm.CloseRequested += ((_, action) =>
                {
                    chosen = action;
                    window.Close();
                });

                window.ShowDialog();
            });

            return Task.FromResult(chosen);
        }
    }
}
