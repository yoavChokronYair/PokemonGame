using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PokemonGame.ViewModels.ViewModelHelper;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace PokemonGame.ViewModels.OnlineBattle
{
    public class TeamViewModel : ViewModelBase
    {
        public string Name { get; }
        public ObservableCollection<PokemonSlotViewModel> Slots { get; }

        public TeamViewModel(string name)
        {
            Name = name;
            Slots = new ObservableCollection<PokemonSlotViewModel>();

            for (int i = 0; i < 6; i++)
                Slots.Add(new PokemonSlotViewModel());
        }
    }

    public class PokemonSlotViewModel : ViewModelBase
    {
        private PokemonViewModel _pokemon;
        public PokemonViewModel Pokemon
        {
            get => _pokemon;
            set
            {
                if (_pokemon != value)
                {
                    _pokemon = value;
                    OnPropertyChanged(nameof(Pokemon));
                    OnPropertyChanged(nameof(IsEmpty));
                }
            }
        }

        public bool IsEmpty => Pokemon == null;
    }

    public class PokemonViewModel : ViewModelBase
    {
        public string Name { get; }
        public string Sprite { get; }
        public string Type { get; }

        public PokemonViewModel(string name, string sprite, string type)
        {
            Name = name;
            Sprite = sprite;
            Type = type;
        }
    }

}
