using PokemonGame.Core.Model.Helper.BattleHelper;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.PokemonCreation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PokemonGame.Model.BattleSystem.Bot
{
    public class WildPokemonBot
    {
        //private PlayerPokemonGeneration playerPokemon { get; set; }
        //private IMoveResult playerMove;
        ////iBotBattle                             
        //public int activePokemonHp { get; set; }
        //public EnemyPokemonGeneration activePokemon { get; set; }
        //private MoveData rivalMove { get; set; }

        //public WildPokemonBot(EnemyPokemonGeneration pokemon, PlayerPokemonGeneration playerPokemon)
        //{
        //    this.playerPokemon = playerPokemon;
        //    activePokemon = pokemon;
        //    activePokemonHp = activePokemon.MaxHP;
        //}
        //public int UpdateData(PlayerPokemonGeneration playerPokemon, IMoveResult playermove, int currentHp)
        //{
        //    this.playerPokemon = playerPokemon;
        //    this.playerMove = playermove;
        //    this.rivalMove = ChooseMove(); 
        //    if (!HasPriority(this.rivalMove,playermove))
        //    {
        //        ReceiveDamage();
        //        if (activePokemonHp < 0)
        //        {
        //            return 0;
        //        }
        //    }
        //    return activePokemonHp;
        //}
        //public void ChooseNextPokemon()
        //{
        //}
        //public bool ShouldSwitchPokemon()
        //{
        //    return false;
        //}
        //public void SwitchPokemon()
        //{
        //   return;
        //}
        //public bool HasPriority(MoveData rivalMove, IMoveResult playerMove)
        //{
        //    if (rivalMove.Priority != playerMove.Priority)
        //        return rivalMove.Priority > playerMove.Priority;

        //    // If equal, fall back to Speed
        //    return this.activePokemon.IVs.Speed > this.playerPokemon.IVs.Speed;
        //}
        //public MoveData ChooseMove()
        //{
        //    List<MoveData> availableMoves = new List<MoveData>();
        //    foreach (var e in activePokemon.Moves)
        //    {
        //        if (e.Value > 0)
        //        {
        //            availableMoves.Add(e.Key);
        //        }
        //    }
        //    if (availableMoves.Count == 0)
        //        return null;
        //    availableMoves = availableMoves.OrderBy(m => m.Power).ToList();
        //    availableMoves.Reverse();
        //    return availableMoves.FirstOrDefault();
        //}
        //public int HealPokemon(string item)
        //{
        //    return activePokemonHp;
        //}
        //public MoveResult ExecuteMove()
        //{
        //    MoveData Movedata = this.rivalMove;
        //    MoveResult moveResult = BattleCalculator.ExecuteMove(playerPokemon, activePokemon, Movedata);
        //    if (BattleCalculator.DoesMoveHit(Movedata, this.activePokemon.StatusType))
        //    {
        //        activePokemon.Moves[Movedata] -= 1;
        //        if (moveResult.IsSwitch)
        //        {
        //            SwitchPokemon();
        //        }
        //        return moveResult;
        //    }
        //    moveResult.Damage = 0;
        //    moveResult.IsSwitch = false;
        //    moveResult.StatusEffect = StatusType.None;
        //    return moveResult;
        //}
        //public void ReceiveDamage()
        //{
        //    activePokemonHp -= playerMove.Damage; // Simple damage logic
        //    if (activePokemon.StatusType != StatusType.None)
        //    {
        //        activePokemon.StatusType = playerMove.StatusEffect;
        //    }
        //}
        //public int EndTurn(bool HasPriority)
        //{
        //    ApplyStatusEffects();
        //    if (HasPriority)
        //    {
        //        ReceiveDamage();
        //    }
        //    if (this.activePokemonHp <= 0)
        //    {
        //        return 0;
        //    }
        //    return this.activePokemonHp;
        //}

        //private void ApplyStatusEffects()
        //{
        //    switch (this.activePokemon.StatusType)
        //    {
        //        case StatusType.Burn:
        //        case StatusType.Poison:
        //            this.activePokemonHp -= (int)(this.activePokemon.MaxHP * 0.0625);
        //            break;
        //    }
        //}



    }
}
