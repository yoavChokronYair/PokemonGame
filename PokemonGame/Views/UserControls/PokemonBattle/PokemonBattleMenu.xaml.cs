using System.Web.UI;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;

namespace PokemonGame.Views.UserControls.PokemonBattle
{
    /// <summary>
    /// Interaction logic for PokemonBattleMenu.xaml
    /// </summary>
    public partial class PokemonBattleMenu : System.Windows.Controls.UserControl
    {
        public PokemonBattleMenu()
        {
            InitializeComponent();
            
        }
        public static readonly DependencyProperty CommandProperty =
    DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(PokemonBattleMenu));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
        public static readonly DependencyProperty FightTextProperty =
    DependencyProperty.Register(nameof(FightText), typeof(string), typeof(PokemonBattleMenu), new PropertyMetadata("FIGHT"));

        public static readonly DependencyProperty BagTextProperty =
            DependencyProperty.Register(nameof(BagText), typeof(string), typeof(PokemonBattleMenu), new PropertyMetadata("BAG"));

        public static readonly DependencyProperty PokemonTextProperty =
            DependencyProperty.Register(nameof(PokemonText), typeof(string), typeof(PokemonBattleMenu), new PropertyMetadata("POKeMON"));

        public static readonly DependencyProperty RunTextProperty =
            DependencyProperty.Register(nameof(RunText), typeof(string), typeof(PokemonBattleMenu), new PropertyMetadata("RUN"));

        public string FightText
        {
            get => (string)GetValue(FightTextProperty);
            set => SetValue(FightTextProperty, value);
        }

        public string BagText
        {
            get => (string)GetValue(BagTextProperty);
            set => SetValue(BagTextProperty, value);
        }

        public string PokemonText
        {
            get => (string)GetValue(PokemonTextProperty);
            set => SetValue(PokemonTextProperty, value);
        }

        public string RunText
        {
            get => (string)GetValue(RunTextProperty);
            set => SetValue(RunTextProperty, value);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this); // Make sure the control receives keyboard input
        }
    }
}
