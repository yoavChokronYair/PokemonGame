using System;
using System.Windows;
using System.Windows.Controls;

namespace PokemonGameModel.Views.UserControls.PokemonBattle
{
    /// <summary>
    /// Interaction logic for PlayersPokemon.xaml
    /// </summary>
    public partial class PlayersPokemon : UserControl
    {
        public PlayersPokemon()
        {
            InitializeComponent();
        }

        // ----------------------------
        // Dependency Properties
        // ----------------------------

        // Current HP
        public double CurrentHp
        {
            get => (double)GetValue(CurrentHpProperty);
            set => SetValue(CurrentHpProperty, value);
        }

        public static readonly DependencyProperty CurrentHpProperty =
            DependencyProperty.Register(
                nameof(CurrentHp),
                typeof(double),
                typeof(PlayersPokemon),
                new PropertyMetadata(0.0, OnHealthChanged)
            );

        // Max HP
        public int MaxHp
        {
            get => (int)GetValue(MaxHpProperty);
            set => SetValue(MaxHpProperty, value);
        }

        public static readonly DependencyProperty MaxHpProperty =
            DependencyProperty.Register(
                nameof(MaxHp),
                typeof(int),
                typeof(PlayersPokemon),
                new PropertyMetadata(100, OnHealthChanged)
            );

        // Name Text
        public string NameText
        {
            get => (string)GetValue(NameTextProperty);
            set => SetValue(NameTextProperty, value);
        }

        public static readonly DependencyProperty NameTextProperty =
            DependencyProperty.Register(
                nameof(NameText),
                typeof(string),
                typeof(PlayersPokemon),
                new PropertyMetadata("")
            );

        // Level Text
        public string LevelText
        {
            get => (string)GetValue(LevelTextProperty);
            set => SetValue(LevelTextProperty, value);
        }

        public static readonly DependencyProperty LevelTextProperty =
            DependencyProperty.Register(
                nameof(LevelText),
                typeof(string),
                typeof(PlayersPokemon),
                new PropertyMetadata("")
            );

        // Gender Symbol
        public string GenderSymbol
        {
            get => (string)GetValue(GenderSymbolProperty);
            set => SetValue(GenderSymbolProperty, value);
        }

        public static readonly DependencyProperty GenderSymbolProperty =
            DependencyProperty.Register(
                nameof(GenderSymbol),
                typeof(string),
                typeof(PlayersPokemon),
                new PropertyMetadata("")
            );

        // ----------------------------
        // Computed Properties
        // ----------------------------

        public double HealthPercent
        {
            get
            {
                if (MaxHp == 0) return 0;
                return Math.Max(0, Math.Min(1, CurrentHp / MaxHp));
            }
        }

        // ----------------------------
        // Callbacks
        // ----------------------------

        private static void OnHealthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PlayersPokemon)d;
            control.MyHPBar.HealthPercent = control.HealthPercent;
        }
    }
}
