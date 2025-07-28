using System;
using System.ComponentModel;
using System.Windows.Input;

namespace PokemonGameModel.ViewModel.ViewModelHelper
{
    public  class ViewModelBase:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyChanged)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyChanged));
        }
    }
}
