using PokemonGameModel.Enums;
using PokemonGameModel.Model.Data;
using PokemonGameModel.Model.Helper;
using PokemonGameModel.Model.PokemonCreation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGameModel.Model.BattleSystem.Player
{
    public class PlayerPokemonBot
    {
        //ToDo:fix healing After adding items
        private int _activePokemonIndex = 0; // Tracks which Pokémon is currently active in battle
        private EnemyPokemonGeneration _RivalPokemon { get; set; }
        private MoveResult _playerMove { get; set; }
        private Dictionary<PlayerPokemonGeneration, (bool, int)> _Team { get; set; }
        //iBotBattle                             
        public int _ActivePokemonHp { get; set; }
        public PlayerPokemonGeneration _ActivePokemon { get; set; }
        public bool DoesNeedToSwitch = false;
        
        public PlayerPokemonBot(List<PlayerPokemonGeneration> team, EnemyPokemonGeneration RivalPokemon)
        {
            //ToDo:add the actual player's pokemon team
            _Team = new Dictionary<PlayerPokemonGeneration, (bool, int)>();
            foreach (PlayerPokemonGeneration pokemon in team)
            {
                _Team.Add(pokemon, (false, pokemon.MaxHP));
            }
            this._RivalPokemon = RivalPokemon;
            _ActivePokemon = team[0];
            _ActivePokemonHp = _ActivePokemon.MaxHP;
        }
        private bool HasProirerty()
        {
            return _ActivePokemon.IVs.Speed > _RivalPokemon.IVs.Speed; // Customize this threshold
        }
        private void ReceiveDamage()
        {
            _ActivePokemonHp -= _playerMove.Damage; // Simple damage logic
            if (_ActivePokemon.StatusType != StatusType.None)
            {
                _ActivePokemon.StatusType = _playerMove.StatusEffect;
            }
        }
        public int UpdateData(EnemyPokemonGeneration playerPokemon, MoveResult Rivalmove, int currentHp)
        {
            this._RivalPokemon = playerPokemon;
            this._playerMove = Rivalmove;
            _ActivePokemonHp = currentHp;
            if (_ActivePokemon.StatusType == StatusType.None)
            {
                _ActivePokemon.StatusType = _playerMove.StatusEffect;
            }
            if (!HasProirerty())
            {
                ReceiveDamage();
                if (_ActivePokemonHp < 0)
                {
                    _Team[_ActivePokemon] = (true, 0);
                    return 0;
                }
            }
            return _ActivePokemonHp;
        }
        public bool ChooseNextPokemon(int index)
        {
            if (!_Team[_ActivePokemon].Item1)
            {
                _activePokemonIndex = index;
                _ActivePokemon = _Team.Keys.ToList()[_activePokemonIndex];
                _ActivePokemonHp = _Team[_ActivePokemon].Item2;
                return true;
            }
            return false;
        }
        
        public int HealPokemon(string item)
        {
            return _ActivePokemonHp;
        }
        public MoveResult ExecuteMove(MoveData moveData)
        {
            if (!_Team[_ActivePokemon].Item1)
            {
                MoveResult moveResult = BattleCalculator.ExecuteMove(_RivalPokemon, _ActivePokemon, moveData);
                if (BattleCalculator.DoesMoveHit(moveData, StatusType.None))
                {
                    if (moveResult.IsSwitch)
                    {

                    }
                    return moveResult;
                }
            }
            return new MoveResult() ;
        }
        public int EndTurn()
        {
            if (HasProirerty())
            {
                ReceiveDamage();
            }
            if (_ActivePokemon.StatusType == StatusType.Burn)
            {
                _ActivePokemonHp -= (int)(_ActivePokemon.MaxHP * 0.0625);
            }
            if (_ActivePokemon.StatusType == StatusType.Poison)
            {
                _ActivePokemonHp -= (int)(_ActivePokemon.MaxHP * 0.0625);

            }
            if (_ActivePokemonHp < 0)
            {
                _Team[_ActivePokemon] = (true, 0);
                return 0;
            }
            
            return _ActivePokemonHp;
        }

    }
}
