using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using PokemonGame.Enums;
using PokemonGame.Model.Data;
using System;
using System.Collections.ObjectModel;

using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace PokemonGame.ViewModel
{
    public class PokemonStatsViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int ID { get; set; } 
        public string Level { get; set; }
        public int Hp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int SpAttack { get; set; }
        public int SpDefense { get; set; }
        public int Speed { get; set; }
        public PokemonType type1 { get; set; }
        public PokemonType type2 { get; set; }
        public BitmapImage Sprite { get; set; }


        public PokemonStatsViewModel(PokemonData data)
        {
          
            Name = data.Name;
            ID = data.Number; 
            Level = "50"; // You can replace with actual value
            Hp = data.HP;
            Attack = data.Attack;
            Defense = data.Defense;
            SpAttack = data.SpAtk;
            SpDefense = data.SpDef;
            Speed = data.Speed;
            type1 = data.Type1;
            type2 = data.Type2;
            Sprite = new BitmapImage(new Uri($"pack://application:,,,/Images/GenOnePokemon/{data.Number}.png"));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
