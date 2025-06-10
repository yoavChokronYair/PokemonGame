using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonGame.Views.PokemonBattle
{
    /// <summary>
    /// Interaction logic for OpponentPokemon.xaml
    /// </summary>
    public partial class OpponentPokemon : UserControl
    {
        public OpponentPokemon()
        {
            InitializeComponent();
        }
        public int CurrentHp
        {
            get => (int)GetValue(CurrentHpProperty);
            set => SetValue(CurrentHpProperty, value);
        }

        public static readonly DependencyProperty CurrentHpProperty =
            DependencyProperty.Register(nameof(CurrentHp), typeof(int), typeof(OpponentPokemon), new PropertyMetadata(0));

        public int MaxHp
        {
            get => (int)GetValue(MaxHpProperty);
            set => SetValue(MaxHpProperty, value);
        }

        public static readonly DependencyProperty MaxHpProperty =
            DependencyProperty.Register(nameof(MaxHp), typeof(int), typeof(OpponentPokemon), new PropertyMetadata(100));

        public string NameText
        {
            get => (string)GetValue(NameTextProperty);
            set => SetValue(NameTextProperty, value);
        }
        public static readonly DependencyProperty NameTextProperty =
                DependencyProperty.Register(nameof(NameText), typeof(string), typeof(OpponentPokemon), new PropertyMetadata(""));


        public string LevelText
        {
            get => (string)GetValue(LevelTextProperty);
            set => SetValue(LevelTextProperty, value);
        }
        // LevelText DependencyProperty
        public static readonly DependencyProperty LevelTextProperty =
            DependencyProperty.Register(nameof(LevelText), typeof(string), typeof(OpponentPokemon), new PropertyMetadata(""));


        public string GenderSymbol
        {
            get => (string)GetValue(GenderSymbolProperty);
            set => SetValue(GenderSymbolProperty, value);
        }

        public static readonly DependencyProperty GenderSymbolProperty =
            DependencyProperty.Register(nameof(GenderSymbol), typeof(string), typeof(OpponentPokemon), new PropertyMetadata("♂"));
    }
}
