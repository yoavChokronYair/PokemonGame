using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Model.BattleSystem.Bot
{
    public class RivalBot : IBotBattle, ITrainer
    {
        private int _activePokemonIndex = 0; // Tracks which Pokémon is currently active in battle
        private PlayerPokemonGeneration _PlayerPokemon {get; set;}
        private  BattleCalculator.MoveResult _playerMove { get; set;}
        private Dictionary<EnemyPokemonGeneration,(bool,int)> _RivalTeam  { get; set; }
        //iBotBattle                             
        public int _ActivePokemonHp { get; set; }
        public EnemyPokemonGeneration _ActivePokemon { get; set;}
        //ITranier
        public int MoneyReward { get; set; } // How much money the player receives on win
        public bool CanRematch { get; set; } // Can this trainer be re-battled?
        public bool IsDeafeted { get; set; } // Used to track game progress
        public bool IsHidden { get; set; } // Set to true to hide trainer until triggered
        public string EncounterLocation { get; set; } // Where this trainer appears
        public bool IsBattleMandatory { get; set; } // If true, auto battle starts on encounter
        public string MusicTheme { get; set; } // Theme music key
        public string SpriteAssetKey { get; set; } // Used to load sprite
        public List<string> ItemRewards { get; set; } // Rewards after victory
        public string Name { get; set; } // Change to match rival name
        public string Description { get; set; }
        public List<string> PreBattleDialog { get; set; }
        public List<string> PostBattleDialogWin { get; set; }
        public List<string> PostBattleDialogLose { get; set; }
        public List<string> MidBattleDialog { get; set; }
        public RivalBot(List<EnemyPokemonGeneration> rivalTeam,PlayerPokemonGeneration playerPokemon)
        {
            foreach(EnemyPokemonGeneration rivalPokemon in rivalTeam)
            {
                _RivalTeam.Add(rivalPokemon, (false, rivalPokemon.MaxHP));
            }
            _PlayerPokemon = playerPokemon;
            _ActivePokemon = rivalTeam[0];
            _ActivePokemonHp = _ActivePokemon.MaxHP;
        }
        public int UpdateData(PlayerPokemonGeneration playerPokemon,BattleCalculator.MoveResult playermove,int currentHp)
        {
            _PlayerPokemon = playerPokemon;
            _playerMove = playermove;
            _ActivePokemonHp = currentHp;
            if(_ActivePokemon.StatusType == StatusType.None)
            {
                _ActivePokemon.StatusType = _playerMove.StatusEffect;
            }
            if (!HasProirerty())
            {
                ReceiveDamage();
                if(_ActivePokemonHp < 0)
                {
                    _RivalTeam[_ActivePokemon] = (true,0);
                }
            }
            return _ActivePokemonHp;
        }
        public bool HasProirerty()
        {
            return _ActivePokemon.IVs.Speed > _PlayerPokemon.IVs.Speed; // Customize this threshold
        }
        public void ChooseNextPokemon()
        {
            for (int i = 0; i < _RivalTeam.Count; i++)
            {
                if (!_RivalTeam[_ActivePokemon].Item1)
                {
                    _activePokemonIndex = i;
                    break;
                }
            }
            _ActivePokemon = _RivalTeam.Keys.ToList()[_activePokemonIndex];
            _ActivePokemonHp = _RivalTeam[_ActivePokemon].Item2;
        }
        public bool ShouldSwitchPokemon()
        {
            if (_RivalTeam[_ActivePokemon].Item1) return true;
            return false;
        }
        public void SwitchPokemon()
        {
            if (ShouldSwitchPokemon())
            {
                ChooseNextPokemon();
            }
        }
        public MoveData ChooseMove()
        {
            List<MoveData> availableMoves = new List<MoveData>();
            foreach (var e in _ActivePokemon.Moves)
            {
                if(e.Value > 0)
                {
                    availableMoves.Add(e.Key);
                }
            }
            if (availableMoves.Count == 0)
                return null;
            return availableMoves.OrderBy(m => m.Power).FirstOrDefault();
        }
        public int HealPokemon(string item)
        {
            return _ActivePokemonHp;
        }
        public (double,StatusType) ExecuteMove()
        {
            if (!_RivalTeam[_ActivePokemon].Item1)
            { 
                MoveData Movedata = ChooseMove();
                BattleCalculator.MoveResult moveResult = BattleCalculator.ExecuteMove(_PlayerPokemon, _ActivePokemon, Movedata);
                if (BattleCalculator.DoesMoveHit(Movedata))
                {
                    _ActivePokemon.Moves[Movedata] -= 1;
                    if (moveResult.IsSwitch)
                    {
                        SwitchPokemon();
                    }
                    return (moveResult.Damage,moveResult.StatusEffect);
                }
            }
            return (0,StatusType.None);
        }
        public void ReceiveDamage()
        {
            _ActivePokemonHp -= _playerMove.Damage; // Simple damage logic
        }
        public int EndTurn()
        {
            if(HasProirerty())
            {
                ReceiveDamage();
            }
            if (_ActivePokemon.StatusType == StatusType.Burn)
            {
                _ActivePokemonHp -= (int)(_ActivePokemon.MaxHP * 0.0625);
            }
            if(_ActivePokemon.StatusType == StatusType.Poison)
            {
                _ActivePokemonHp -= (int)(_ActivePokemon.MaxHP * 0.0625);
                 
            }
            if (_ActivePokemonHp < 0)
            {
                _RivalTeam[_ActivePokemon] = (true, 0);
            }
            SwitchPokemon();
            return _ActivePokemonHp;
        }
    }
}
