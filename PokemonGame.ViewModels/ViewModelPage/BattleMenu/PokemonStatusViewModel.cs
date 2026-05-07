using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using PokemonGame.ViewModels.ViewModelHelper;

namespace PokemonGame.ViewModels.ViewModelPage.BattleMenu
{
    public class PokemonBattleStatusViewModel : ViewModelBase
    {
        private TaskCompletionSource<bool>? _animationTcs;
        private string _pokemonName = "";
        private int _level;
        private int _currentHp;
        private int _maxHp;
        private string _gender = "None";
        private int _pokedexId;
        private string _statusCondition = "";

        private double _animatedHpPercentage = 1;
        private double _targetHpPercentage = 1;

        private CancellationTokenSource? _hpAnimationToken;

        public int PokedexId
        {
            get => _pokedexId;
            set
            {
                if (_pokedexId != value)
                {
                    _pokedexId = value;

                    OnPropertyChanged(nameof(PokedexId));

                    SyncHpInstantly();
                }
            }
        }

        public string PokemonName
        {
            get => _pokemonName;
            set
            {
                if (_pokemonName != value)
                {
                    _pokemonName = value;

                    OnPropertyChanged(nameof(PokemonName));
                }
            }
        }

        public int Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    _level = value;

                    OnPropertyChanged(nameof(Level));
                }
            }
        }

        public int CurrentHP
        {
            get => _currentHp;
            set
            {
                if (_currentHp != value)
                {
                    _currentHp = value;

                    OnPropertyChanged(nameof(CurrentHP));
                    OnPropertyChanged(nameof(HpPercentage));
                    OnPropertyChanged(nameof(HPColor));
                    OnPropertyChanged(nameof(HPStatusText));

                    UpdateHpTarget();
                }
            }
        }

        public Task WaitForHpAnimation()
        {
            return _animationTcs?.Task ?? Task.CompletedTask;
        }
        public int MaxHP
        {
            get => _maxHp;
            set
            {
                if (_maxHp != value)
                {
                    _maxHp = value;

                    OnPropertyChanged(nameof(MaxHP));
                    OnPropertyChanged(nameof(HpPercentage));
                    OnPropertyChanged(nameof(HPColor));
                    OnPropertyChanged(nameof(HPStatusText));

                    SyncHpInstantly();
                }
            }
        }

        public string Gender
        {
            get => _gender;
            set
            {
                if (_gender != value)
                {
                    _gender = value;

                    OnPropertyChanged(nameof(Gender));
                    OnPropertyChanged(nameof(GenderSymbol));
                    OnPropertyChanged(nameof(GenderColor));
                }
            }
        }

        public string StatusCondition
        {
            get => _statusCondition;
            set
            {
                if (_statusCondition != value)
                {
                    _statusCondition = value;

                    OnPropertyChanged(nameof(StatusCondition));
                }
            }
        }

        public double HpPercentage =>
            MaxHP > 0
                ? (double)CurrentHP / MaxHP
                : 0;

        public double AnimatedHpPercentage
        {
            get => _animatedHpPercentage;
            set
            {
                if (_animatedHpPercentage != value)
                {
                    _animatedHpPercentage = value;

                    OnPropertyChanged(nameof(AnimatedHpPercentage));
                }
            }
        }

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

        public string GenderSymbol =>
            Gender == "Male"
                ? "♂"
                : Gender == "Female"
                    ? "♀"
                    : "";

        public Brush GenderColor =>
            Gender == "Male"
                ? Brushes.DeepSkyBlue
                : Brushes.HotPink;

        private void UpdateHpTarget()
        {
            if (MaxHP <= 0)
            {
                return;
            }

            _targetHpPercentage = (double)CurrentHP / MaxHP;

            StartHpAnimation();
        }

        private void SyncHpInstantly()
        {
            if (MaxHP <= 0)
            {
                return;
            }

            _targetHpPercentage = (double)CurrentHP / MaxHP;

            AnimatedHpPercentage = _targetHpPercentage;
        }

        private async void StartHpAnimation()
        {
            _hpAnimationToken?.Cancel();

            _hpAnimationToken = new CancellationTokenSource();

            CancellationToken token = _hpAnimationToken.Token;

            _animationTcs = new TaskCompletionSource<bool>();

            try
            {
                while (Math.Abs(AnimatedHpPercentage - _targetHpPercentage) > 0.001)
                {
                    if (token.IsCancellationRequested)
                    {
                        _animationTcs.TrySetResult(true);
                        return;
                    }

                    double speed = 0.015;

                    if (AnimatedHpPercentage > _targetHpPercentage)
                    {
                        AnimatedHpPercentage -= speed;

                        if (AnimatedHpPercentage < _targetHpPercentage)
                        {
                            AnimatedHpPercentage = _targetHpPercentage;
                        }
                    }
                    else
                    {
                        AnimatedHpPercentage += speed;

                        if (AnimatedHpPercentage > _targetHpPercentage)
                        {
                            AnimatedHpPercentage = _targetHpPercentage;
                        }
                    }

                    await Task.Delay(12, token);
                }

                AnimatedHpPercentage = _targetHpPercentage;
            }
            catch
            {
            }

            _animationTcs.TrySetResult(true);
        }
    }
}