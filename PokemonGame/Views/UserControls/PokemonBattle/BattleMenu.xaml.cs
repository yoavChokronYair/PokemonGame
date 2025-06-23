using PokemonGame.Views.Controls;
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


namespace PokemonGame.Views.UserControls.PokemonBattle
{
    /// <summary>
    /// Interaction logic for BattleMenu.xaml
    /// </summary>
    public partial class BattleMenu : UserControl
    {
        public static readonly DependencyProperty Move1Property =
        DependencyProperty.Register(nameof(Move1), typeof(string), typeof(BattleMenu), new PropertyMetadata("-"));

        public static readonly DependencyProperty Move2Property =
            DependencyProperty.Register(nameof(Move2), typeof(string), typeof(BattleMenu), new PropertyMetadata("-"));

        public static readonly DependencyProperty Move3Property =
            DependencyProperty.Register(nameof(Move3), typeof(string), typeof(BattleMenu), new PropertyMetadata("-"));

        public static readonly DependencyProperty Move4Property =
            DependencyProperty.Register(nameof(Move4), typeof(string), typeof(BattleMenu), new PropertyMetadata("-"));

        public string Move1
        {
            get => (string)GetValue(Move1Property);
            set => SetValue(Move1Property, value);
        }

        public string Move2
        {
            get => (string)GetValue(Move2Property);
            set => SetValue(Move2Property, value);
        }

        public string Move3
        {
            get => (string)GetValue(Move3Property);
            set => SetValue(Move3Property, value);
        }

        public string Move4
        {
            get => (string)GetValue(Move4Property);
            set => SetValue(Move4Property, value);
        }

        public BattleMenu()
        {
            InitializeComponent();
        }
        public event EventHandler<string> MoveClicked;

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                MoveClicked?.Invoke(this, textBlock.Text);
            }
        }
        public int CurrentPP
        {
            get => (int)GetValue(CurrentPPProperty);
            set => SetValue(CurrentPPProperty, value);
        }

        public static readonly DependencyProperty CurrentPPProperty =
            DependencyProperty.Register(nameof(CurrentPP), typeof(int), typeof(BattleMenu), new PropertyMetadata(0));
        public int MaxPP
        {
            get => (int)GetValue(MaxPPProperty);
            set => SetValue(MaxPPProperty, value);
        }

        public static readonly DependencyProperty MaxPPProperty =
            DependencyProperty.Register(nameof(MaxPP), typeof(int), typeof(BattleMenu), new PropertyMetadata(0));
        public string Type
        {
            get => (string)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(string), typeof(BattleMenu), new PropertyMetadata("None"));

        public BackgroundType BackgroundType
        {
            get => (BackgroundType)GetValue(BackgroundTypeProperty);
            set => SetValue(BackgroundTypeProperty, value);
        }

        public static readonly DependencyProperty BackgroundTypeProperty =
            DependencyProperty.Register("BackgroundType", typeof(BackgroundType), typeof(BattleMenu), new PropertyMetadata(BackgroundType.White));

    }
}
