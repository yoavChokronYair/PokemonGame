using PokemonGame.Views.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace PokemonGame.Views.UserControls.PokemonBattle
{
    /// <summary>
    /// Interaction logic for BattleMenu.xaml
    /// </summary>
    public partial class BattleMenuMoveset : UserControl
    {
        public static readonly DependencyProperty Move1Property =
        DependencyProperty.Register(nameof(Move1), typeof(string), typeof(BattleMenuMoveset), new PropertyMetadata("-"));

        public static readonly DependencyProperty Move2Property =
            DependencyProperty.Register(nameof(Move2), typeof(string), typeof(BattleMenuMoveset), new PropertyMetadata("-"));

        public static readonly DependencyProperty Move3Property =
            DependencyProperty.Register(nameof(Move3), typeof(string), typeof(BattleMenuMoveset), new PropertyMetadata("-"));

        public static readonly DependencyProperty Move4Property =
            DependencyProperty.Register(nameof(Move4), typeof(string), typeof(BattleMenuMoveset), new PropertyMetadata("-"));
      
        public static readonly DependencyProperty DirectionCommandProperty =
           DependencyProperty.Register(nameof(DirectionCommand), typeof(ICommand), typeof(BattleMenuMoveset));

        public ICommand DirectionCommand
        {
            get => (ICommand)GetValue(DirectionCommandProperty);
            set => SetValue(DirectionCommandProperty, value);
        }

        public static readonly DependencyProperty ConfirmMoveCommandProperty =
            DependencyProperty.Register(nameof(ConfirmMoveCommand), typeof(ICommand), typeof(BattleMenuMoveset));

        public ICommand ConfirmMoveCommand
        {
            get => (ICommand)GetValue(ConfirmMoveCommandProperty);
            set => SetValue(ConfirmMoveCommandProperty, value);
        }

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(BattleMenuMoveset));

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
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

        public BattleMenuMoveset()
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
            DependencyProperty.Register(nameof(CurrentPP), typeof(int), typeof(BattleMenuMoveset), new PropertyMetadata(0));
        public int MaxPP
        {
            get => (int)GetValue(MaxPPProperty);
            set => SetValue(MaxPPProperty, value);
        }

        public static readonly DependencyProperty MaxPPProperty =
            DependencyProperty.Register(nameof(MaxPP), typeof(int), typeof(BattleMenuMoveset), new PropertyMetadata(0));
        public string Type
        {
            get => (string)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(string), typeof(BattleMenuMoveset), new PropertyMetadata("None"));

        public BackgroundType BackgroundType
        {
            get => (BackgroundType)GetValue(BackgroundTypeProperty);
            set => SetValue(BackgroundTypeProperty, value);
        }

        public static readonly DependencyProperty BackgroundTypeProperty =
            DependencyProperty.Register("BackgroundType", typeof(BackgroundType), typeof(BattleMenuMoveset), new PropertyMetadata(BackgroundType.White));


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this);
        }
    }
}
