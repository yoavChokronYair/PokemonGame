using CommunityToolkit.Mvvm.ComponentModel;

namespace PokemonGame.ViewModel.BattleMenu
{
    public class MenuSelectionViewModel : ObservableObject
    {
        public int Rows => 2;
        public int Columns => 2;

        private int selectedRow;
        public int SelectedRow
        {
            get => selectedRow;
            set => SetProperty(ref selectedRow, value);
        }

        private int selectedCol;
        public int SelectedCol
        {
            get => selectedCol;
            set => SetProperty(ref selectedCol, value);
        }
    }
}
