using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace PokemonGame.ViewModel.ViewModelHelper
{
    public class KeyDownToCommandBehavior:TriggerAction<UIElement>
    {
        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(KeyDownToCommandBehavior), new PropertyMetadata(null));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void Invoke(object parameter)
        {
            if (parameter is KeyEventArgs args && Command?.CanExecute(args) == true)
            {
                Command.Execute(args);
            }
        }
    }
}
