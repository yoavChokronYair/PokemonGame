using System.Windows.Media;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class PokemonBattleStatusViewModel : ViewModelBase
    {
        private string _pokemonName = "";
        private int _level;
        private int _currentHp;
        private int _maxHp;
        private string _gender = "None";
        private int _pokedexId;
        private string _statusCondition = "";
    
        public int PokedexId
        {
            get => _pokedexId;
            set => SetProperty(ref _pokedexId, value);
        }


        public string PokemonName { get => _pokemonName; set => SetProperty(ref _pokemonName, value); }
        public int Level { get => _level; set => SetProperty(ref _level, value); }

        public int CurrentHP
        {
            get => _currentHp;
            set { if (SetProperty(ref _currentHp, value)) { OnPropertyChanged(nameof(HpPercentage)); OnPropertyChanged(nameof(HPColor)); OnPropertyChanged(nameof(HPStatusText)); } }
        }

        public int MaxHP
        {
            get => _maxHp;
            set { if (SetProperty(ref _maxHp, value)) { OnPropertyChanged(nameof(HpPercentage)); OnPropertyChanged(nameof(HPColor)); OnPropertyChanged(nameof(HPStatusText)); } }
        }

        public string Gender
        {
            get => _gender;
            set { if (SetProperty(ref _gender, value)) { OnPropertyChanged(nameof(GenderSymbol)); OnPropertyChanged(nameof(GenderColor)); } }
        }
        public string StatusCondition
        {
            get => _statusCondition;
            set { if (SetProperty(ref _statusCondition, value)) { OnPropertyChanged(nameof(StatusCondition)); } }
        }
        public double HpPercentage => MaxHP > 0 ? (double)CurrentHP / MaxHP : 0;
        public string HPStatusText => $"{CurrentHP}/{MaxHP}";

        public Brush HPColor
        {
            get
            {
                double r = HpPercentage;
                if (r > 0.5)
                {
                    return new SolidColorBrush(Color.FromRgb(80, 240, 144));
                }

                if (r > 0.2)
                {
                    return new SolidColorBrush(Color.FromRgb(248, 224, 56));
                }

                return new SolidColorBrush(Color.FromRgb(248, 88, 56));
            }
        }

        public string GenderSymbol => Gender == "Male" ? "♂" : Gender == "Female" ? "♀" : "";
        public Brush GenderColor => Gender == "Male" ? Brushes.DeepSkyBlue : Brushes.HotPink;
    }
}
