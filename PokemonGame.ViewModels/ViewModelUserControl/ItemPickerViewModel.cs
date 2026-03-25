using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelUserControl
{
    public class ItemPickerViewModel : ViewModelBase
    {
        private readonly TeamBuilderState _state;
        public ObservableCollection<ItemData> AllItems { get; }

        private string _itemSearchText = string.Empty;
        public string ItemSearchText
        {
            get => _itemSearchText;
            set
            {
                if (SetProperty(ref _itemSearchText, value))
                    OnPropertyChanged(nameof(FilteredItems));
            }
        }

        public IEnumerable<ItemData> FilteredItems =>
            string.IsNullOrWhiteSpace(ItemSearchText)
                ? AllItems
                : AllItems.Where(i => i.Name.IndexOf(ItemSearchText, StringComparison.OrdinalIgnoreCase) >= 0);

        private ItemData _selectedItem;
        public ItemData SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                    ConfirmItemCommand.Execute(null);
            }
        }

        public RelayCommand ConfirmItemCommand { get; }

        public ItemPickerViewModel(TeamBuilderState state, TeamBuilderService service,
            ObservableCollection<ItemData> allItems)
        {
            _state = state;
            AllItems = allItems;

            _state.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TeamBuilderState.IsItemPickerOpen) && _state.IsItemPickerOpen)
                {
                    ItemSearchText = string.Empty;
                    OnPropertyChanged(nameof(FilteredItems));
                }
            };

            ConfirmItemCommand = new RelayCommand(() =>
            {
                if (_state.SelectedPokemon == null || SelectedItem == null) return;
                _state.SelectedPokemon.HeldItemName = SelectedItem.Name;
                _selectedItem = null;
                OnPropertyChanged(nameof(SelectedItem));
                _state.IsItemPickerOpen = false;
            });
        }
    }
}
