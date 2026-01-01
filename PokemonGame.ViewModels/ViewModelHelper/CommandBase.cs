using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.ViewModels.ViewModelHelper
{
    public abstract class CommandBase
    {
        public event EventHandler? CanExecuteChanged;

        public CommandBase(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            
        }
        public virtual bool CanExecute(object? parameter)
        {
            return true;
        }

        public abstract void Execute(object? parameter);
        protected void OnCanExecuteChanged() 
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
