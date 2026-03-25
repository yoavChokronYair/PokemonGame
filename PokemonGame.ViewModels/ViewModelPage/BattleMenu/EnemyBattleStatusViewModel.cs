using System.Windows.Media;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class EnemyBattleStatusViewModel : ViewModelBase
    {
        private string _pokemonName = "FOE PIKACHU";
        private int _level = 20;
        private int _currentHp = 50;
        private int _maxHp = 50;
        private string _gender = "Female"; // Options: "Male", "Female", "None"

        public string PokemonName
        {
            get => _pokemonName;
            set => SetProperty(ref _pokemonName, value);
        }

        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public int CurrentHP
        {
            get => _currentHp;
            set
            {
                if (SetProperty(ref _currentHp, value))
                {
                    OnPropertyChanged(nameof(HpPercentage));
                    OnPropertyChanged(nameof(HPColor));
                }
            }
        }

        public int MaxHP
        {
            get => _maxHp;
            set
            {
                if (SetProperty(ref _maxHp, value))
                {
                    OnPropertyChanged(nameof(HpPercentage));
                    OnPropertyChanged(nameof(HPColor));
                }
            }
        }

        public string Gender
        {
            get => _gender;
            set
            {
                if (SetProperty(ref _gender, value))
                {
                    OnPropertyChanged(nameof(GenderSymbol));
                    OnPropertyChanged(nameof(GenderColor));
                }
            }
        }

        // --- Logic Helper Properties ---

        // Returns ratio (0.0 to 1.0) for the ScaleTransform
        public double HpPercentage => MaxHP > 0 ? (double)CurrentHP / MaxHP : 0;

        // Automatically changes the color of the bar based on health percentage
        public Brush HPColor
        {
            get
            {
                double ratio = HpPercentage;
                if (ratio > 0.5) return new SolidColorBrush(Color.FromRgb(80, 240, 144));  // Green
                if (ratio > 0.2) return new SolidColorBrush(Color.FromRgb(248, 224, 56)); // Yellow
                return new SolidColorBrush(Color.FromRgb(248, 88, 56));                  // Red
            }
        }

        // Returns symbol based on string value
        public string GenderSymbol => Gender == "Male" ? "♂" : (Gender == "Female" ? "♀" : "");

        // Returns blue for male, pink for female
        public Brush GenderColor => Gender == "Male" ? Brushes.DeepSkyBlue : Brushes.HotPink;
    }
}
