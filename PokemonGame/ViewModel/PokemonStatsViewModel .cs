using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using PokemonGame.Enums;
using PokemonGame.Model.Data;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.Views;
using System;
using System.Collections.ObjectModel;

using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace PokemonGame.ViewModel
{
    public class PokemonStatsViewModel : INotifyPropertyChanged
    {
        public string RivalName { get; set; }
        public int RivalID { get; set; } 
        public string RivalLevel { get; set; }
        public int RivalHp { get; set; }
        public int RivalAttack { get; set; }
        public int RivalDefense { get; set; }
        public int RivalSpAttack { get; set; }
        public int RivalSpDefense { get; set; }
        public int RivalSpeed { get; set; }
        public PokemonType Rivaltype1 { get; set; }
        public PokemonType Rivaltype2 { get; set; }
        public BitmapImage RivalSprite { get; set; }
        public string TeamName { get; set; }
        public int TeamID { get; set; } 
        public string TeamLevel { get; set; }
        public int TeamHp { get; set; }
        public int TeamAttack { get; set; }
        public int TeamDefense { get; set; }
        public int TeamSpAttack { get; set; }
        public int TeamSpDefense { get; set; }
        public int TeamSpeed { get; set; }
        public PokemonType Teamtype1 { get; set; }
        public PokemonType Teamtype2 { get; set; }
        public BitmapImage TeamSprite { get; set; }


        public PokemonStatsViewModel(PokemonData Rival,PokemonData Team,WildPokemonGenartion TeamEncounter,WildPokemonGenartion RivalEncounter)
        {
          
            RivalName = Rival.Name.ToString();
            RivalID = Rival.Number; 
            RivalLevel = RivalEncounter.Level.ToString(); // You can replace with actual value
            RivalHp = Rival.HP;
            RivalAttack = Rival.Attack;
            RivalDefense = Rival.Defense;
            RivalSpAttack = Rival.SpAtk;
            RivalSpDefense = Rival.SpDef;
            RivalSpeed = Rival.Speed;
            Rivaltype1 = Rival.Type1;
            Rivaltype2 = Rival.Type2;
            RivalSprite = new BitmapImage(new Uri($"pack://application:,,,/Images/GenOnePokemon/{Rival.Number}.png"));
            
            TeamName = Team.Name;
            TeamID = Team.Number;
            TeamLevel = TeamEncounter.Level.ToString(); // You can replace with actual value
            TeamHp = Team.HP;
            TeamAttack = Team.Attack;
            TeamDefense = Team.Defense;
            TeamSpAttack = Team.SpAtk;
            TeamSpDefense = Team.SpDef;
            TeamSpeed = Team.Speed;
            Teamtype1 = Team.Type1;
            Teamtype2 = Team.Type2;
            TeamSprite = new BitmapImage(new Uri($"pack://application:,,,/Images/GenOnePokemon/{Team.Number}.png"));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
