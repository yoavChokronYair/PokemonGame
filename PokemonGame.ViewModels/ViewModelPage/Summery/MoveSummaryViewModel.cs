using System.Collections.ObjectModel;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.Summery
{
    public class MoveSummaryViewModel : ViewModelBase
    {
        private MoveSlotViewModel _selectedMove;
        private string _pokemonName;
        private string _pokemonType1;
        private string _pokemonType2;

        // The list of 4 moves the Pokemon knows
        public ObservableCollection<MoveSlotViewModel> KnownMoves { get; set; }

        public MoveSlotViewModel SelectedMove
        {
            get => _selectedMove;
            set
            {
                _selectedMove = value;
                OnPropertyChanged(nameof(SelectedMove));
            }
        }

        public string PokemonName { get => _pokemonName; set { _pokemonName = value; OnPropertyChanged(nameof(PokemonName)); } }

        public MoveSummaryViewModel()
        {
            // Initialize with dummy data for testing
            PokemonName = "CHARIZARD";

            KnownMoves = new ObservableCollection<MoveSlotViewModel>
        {
            new MoveSlotViewModel { Name = "FLAMETHROWER", Type = "FIRE", CurrentPp = 15, MaxPp = 15, RawPower = 90, RawAccuracy = 100, Description = "The target is scorched with an intense blast of fire. May leave a burn." },
            new MoveSlotViewModel { Name = "DRAGON CLAW", Type = "DRAGON", CurrentPp = 15, MaxPp = 15, RawPower = 80, RawAccuracy = 100, Description = "The user slashes the target with sharp claws." },
            new MoveSlotViewModel { Name = "FLY", Type = "FLYING", CurrentPp = 15, MaxPp = 15, RawPower = 90, RawAccuracy = 95, Description = "The user soars, then strikes on its next turn." },
            new MoveSlotViewModel { Name = "SLASH", Type = "NORMAL", CurrentPp = 20, MaxPp = 20, RawPower = 70, RawAccuracy = 100, Description = "The target is attacked with a slash. Critical hits land easily." }
        };

            // Default to the first move
            SelectedMove = KnownMoves[0];
        }
    }
    
    public class MoveSlotViewModel : ViewModelBase
    {
        private string _name;
        private string _type; // "Electric", "Fire", etc.
        private int _currentPp;
        private int _maxPp;
        private string _description;
        private int _power;
        private int _accuracy;
        private string _category; // Physical, Special, Status

        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public string Type { get => _type; set { _type = value; OnPropertyChanged(nameof(Type)); } }
        public int CurrentPp { get => _currentPp; set { _currentPp = value; OnPropertyChanged(nameof(CurrentPp)); } }
        public int MaxPp { get => _maxPp; set { _maxPp = value; OnPropertyChanged(nameof(MaxPp)); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }

        // Use strings for Power/Accuracy to handle the "---" case easily
        public string Power => _power > 0 ? _power.ToString() : "---";
        public string Accuracy => _accuracy > 0 ? _accuracy.ToString() : "---";

        public string Category { get => _category; set { _category = value; OnPropertyChanged(nameof(Category)); } }

        // Logic to determine PP text color (Red if low, white if normal)
        public string PpColor => (double)CurrentPp / MaxPp <= 0.2 ? "#FF4500" : "#FFFFFF";
        public int RawPower { get => _power; set { _power = value; OnPropertyChanged(nameof(Power)); } }
        public int RawAccuracy { get => _accuracy; set { _accuracy = value; OnPropertyChanged(nameof(Accuracy)); } }
    }
}
