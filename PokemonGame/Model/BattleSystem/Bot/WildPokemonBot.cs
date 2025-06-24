using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Model.BattleSystem.Bot
{
    public class WildPokemonBot : IBotBattle
    {
        private PlayerPokemonGeneration PlayerPokemon { get; set; }
        private IMoveResult playerMove;
        //iBotBattle                             
        public int _ActivePokemonHp { get; set; }
        public EnemyPokemonGeneration _ActivePokemon { get; set; }
     
        public WildPokemonBot(EnemyPokemonGeneration pokemon, PlayerPokemonGeneration playerPokemon)
        {
            this.PlayerPokemon = playerPokemon;
            _ActivePokemon = pokemon;
            _ActivePokemonHp = _ActivePokemon.MaxHP;
        }
        public int UpdateData(PlayerPokemonGeneration playerPokemon, IMoveResult playermove, int currentHp)
        {
            this.PlayerPokemon = playerPokemon;
            this.playerMove = playermove;
           
            if (!HasProirerty())
            {
                ReceiveDamage();
                if (_ActivePokemonHp < 0)
                {
                    return 0;
                }
            }
            return _ActivePokemonHp;
        }
        public bool HasProirerty()
        {
            return _ActivePokemon.IVs.Speed > PlayerPokemon.IVs.Speed; // Customize this threshold
        }
        public void ChooseNextPokemon()
        {
        }
        public bool ShouldSwitchPokemon()
        {
            return false;
        }
        public void SwitchPokemon()
        {
        }
        public MoveData ChooseMove()
        {
            List<MoveData> availableMoves = new List<MoveData>();
            foreach (var e in _ActivePokemon.Moves)
            {
                if (e.Value > 0)
                {
                    availableMoves.Add(e.Key);
                }
            }
            if (availableMoves.Count == 0)
                return null;
            availableMoves = availableMoves.OrderBy(m => m.Power).ToList();
            availableMoves.Reverse();
            return availableMoves.FirstOrDefault();
        }
        public int HealPokemon(string item)
        {
            return _ActivePokemonHp;
        }
        public MoveResult ExecuteMove()
        {
            MoveData Movedata = ChooseMove();
            MoveResult moveResult = BattleCalculator.ExecuteMove(PlayerPokemon, _ActivePokemon, Movedata);
            if (BattleCalculator.DoesMoveHit(Movedata))
            {
                _ActivePokemon.Moves[Movedata] -= 1;
                if (moveResult.IsSwitch)
                {
                    SwitchPokemon();
                }
                return moveResult;
            }
            moveResult.Damage = 0;
            moveResult.IsSwitch = false;
            moveResult.StatusEffect = StatusType.None;
            return moveResult;
        }
        public void ReceiveDamage()
        {
            _ActivePokemonHp -= playerMove.Damage; // Simple damage logic
            if (_ActivePokemon.StatusType != StatusType.None)
            {
                _ActivePokemon.StatusType = playerMove.StatusEffect;
            }
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
                return 0;
            }
            return _ActivePokemonHp;
        }
    }
}
