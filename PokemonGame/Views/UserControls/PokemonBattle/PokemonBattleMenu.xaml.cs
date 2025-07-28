using System.Windows;
using System.Windows.Input;

namespace PokemonGameModel.Views.UserControls.PokemonBattle
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
        public static readonly DependencyProperty DirectionCommandProperty =
           DependencyProperty.Register(nameof(DirectionCommand), typeof(ICommand), typeof(PokemonBattleMenu));
        public ICommand DirectionCommand
        {
            get => (ICommand)GetValue(DirectionCommandProperty);
            set => SetValue(DirectionCommandProperty, value);
        }

        public static readonly DependencyProperty ConfirmMoveCommandProperty =
            DependencyProperty.Register(nameof(ConfirmMoveCommand), typeof(ICommand), typeof(PokemonBattleMenu));

        public ICommand ConfirmMoveCommand
        {
            get => (ICommand)GetValue(ConfirmMoveCommandProperty);
            set => SetValue(ConfirmMoveCommandProperty, value);
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
