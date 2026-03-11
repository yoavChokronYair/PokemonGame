using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class PokemonBattleMenuViewModel : ViewModelBase
    {
        private readonly NavigationStore _NavigationStore;


        public ObservableCollection<MenuItemViewModel> MenuItems { get; }
        public MenuSelectionViewModel MenuSelection { get; }

        public ICommand DirectionCommand { get; }
        public ICommand ConfirmCommand { get; }

        public PokemonBattleMenuViewModel(NavigationStore navigationStore, NavigationStore navigation)
        {
            _NavigationStore = navigationStore;
            MenuSelection = new MenuSelectionViewModel();
            MenuItems = new ObservableCollection<MenuItemViewModel>
        {
            new MenuItemViewModel("FIGHT"),
            new MenuItemViewModel("BAG"),
            new MenuItemViewModel("POKeMON"),
            new MenuItemViewModel("RUN")
        };

            DirectionCommand = new RelayCommand<string>(OnDirectionInput);
            //ConfirmCommand = new RelayCommand(OnConfirm);

            UpdateSelection();
        }


        private void OnDirectionInput(string direction)
        {
            int row = MenuSelection.SelectedRow;
            int col = MenuSelection.SelectedCol;

            switch (direction)
            {
                case "Up": if (row > 0) { row--; } break;
                case "Down": if (row < 1) { row++; } break;
                case "Left": if (col > 0) { col--; } break;
                case "Right": if (col < 1) { col++; } break;
            }

            MenuSelection.SelectedRow = row;
            MenuSelection.SelectedCol = col;
            UpdateSelection();
        }

        //private void OnConfirm()
        //{
        //    int index = MenuSelection.SelectedRow * 2 + MenuSelection.SelectedCol;
        //    string selected = MenuItems[index].Label;

        //    if (selected == "FIGHT")
        //    {
        //        _NavigationStore.CurrentViewModel = new PokemonBattleMovesetMenuViewModel(_NavigationStore, WildPokemonBattleViewModel);
        //    }
        //    if (selected == "RUN")
        //    {
        //       WildPokemonBattleViewModel._PageNavigationStore.CurrentViewModel = WildPokemonBattleViewModel._mainWindow;
        //    }
        //}

        private void UpdateSelection()
        {
            for (int i = 0; i < MenuItems.Count; i++)
            {
                int row = i / 2;
                int col = i % 2;
                MenuItems[i].IsSelected = row == MenuSelection.SelectedRow && col == MenuSelection.SelectedCol;
            }
        }
    }

}
