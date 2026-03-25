using System.Windows.Media;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class PokemonBattleStatusViewModel : ViewModelBase
    {
        private string _pokemonName = "PIKACHU";
        private int _level = 25;
        private int _currentHp = 22;
        private int _maxHp = 22;
        private string _gender = "Male"; // "Male", "Female", or "None"

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
                    // Refresh all properties that depend on CurrentHP
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

        // --- Logic Properties ---

        // Returns 1.0 for full, 0.0 for empty
        public double HpPercentage => MaxHP > 0 ? (double)CurrentHP / MaxHP : 0;

        // Returns the GBA-accurate HP colors
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

        // Gender Symbol Logic
        public string GenderSymbol => Gender == "Male" ? "♂" : (Gender == "Female" ? "♀" : "");

        // Gender Color Logic
        public Brush GenderColor => Gender == "Male" ? Brushes.DeepSkyBlue : Brushes.HotPink;

        // Note: We use MultiBinding in XAML for the "22/22" text, 
        // but if you prefer a single property, use this:
        public string HPStatusText => $"{CurrentHP}/{MaxHP}";
    }
}
