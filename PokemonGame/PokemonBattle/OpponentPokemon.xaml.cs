using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace PokemonGame.Views.UserControls.PokemonBattle
{
    public partial class OpponentPokemon : UserControl, INotifyPropertyChanged
    {
        public OpponentPokemon()
        {
            InitializeComponent();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
                typeof(OpponentPokemon),
                new PropertyMetadata(0.0, OnHpChanged)
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
                typeof(OpponentPokemon),
                new PropertyMetadata(100, OnHpChanged)
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
                typeof(OpponentPokemon),
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
                typeof(OpponentPokemon),
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
                typeof(OpponentPokemon),
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

        private static void OnHpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (OpponentPokemon)d;
            control.OnPropertyChanged(nameof(HealthPercent));
        }

        // ----------------------------
        // Event Handlers
        // ----------------------------

        private void MyHPBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Placeholder for future logic
        }
    }
}
