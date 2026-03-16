using System.Collections.Generic;
using System.Collections.ObjectModel;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.OnlineBattle
{
    public class TeamViewModel : ViewModelBase
    {
        public string Name { get; }
        public ObservableCollection<PokemonSlotViewModel> Slots { get; }

        public TeamViewModel(string name)
        {
            Name = name;
            Slots = new ObservableCollection<PokemonSlotViewModel>();

            for (int i = 0; i < 6; i++)
            {
                // No service needed here
                Slots.Add(new PokemonSlotViewModel());
            }
        }
    }

    public class PokemonSlotViewModel : ViewModelBase
    {
        private PokemonViewModel _pokemon;
        public PokemonViewModel Pokemon
        {
            get => _pokemon;
            set
            {
                if (SetProperty(ref _pokemon, value))
                {
                    OnPropertyChanged(nameof(IsEmpty));
                    LoadMockData(); // Now calls static data instead of service
                }
            }
        }

        public bool IsEmpty => Pokemon == null;

        public ObservableCollection<string> AvailableAbilities { get; } = new();
        public ObservableCollection<string> AvailableItems { get; } = new();
        public ObservableCollection<string> SelectedMoves { get; } = new() { "", "", "", "" };

        public PokemonSlotViewModel() { }

        private void LoadMockData()
        {
            AvailableAbilities.Clear();
            AvailableItems.Clear();

            if (Pokemon == null) return;

            // Mock Data for testing the UI
            AvailableAbilities.Add("Overgrow");
            AvailableAbilities.Add("Chlorophyll");

            AvailableItems.Add("Miracle Seed");
            AvailableItems.Add("Leftovers");
            AvailableItems.Add("Life Orb");
        }
    }

    public class PokemonViewModel : ViewModelBase
    {
        public string Name { get; }
        public string Sprite { get; }
        public string Type { get; }

        public PokemonViewModel(string name, string sprite, string type)
        {
            Name = name;
            Sprite = sprite;
            Type = type;
        }
    }
}