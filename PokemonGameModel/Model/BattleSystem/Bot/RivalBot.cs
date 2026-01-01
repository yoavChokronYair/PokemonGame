using PokemonGame.Constants;
using PokemonGame.Core.Model.Helper.BattleHelper;
using PokemonGame.Enums;
using PokemonGame.Interface;
using PokemonGame.Model.PokemonCreation;
using PokemonGame.Services.GameData;

namespace PokemonGame.Model.BattleSystem.Bot
{
    public class RivalBot
    {
    //    private Dictionary<EnemyPokemonGeneration,(bool,int)> rivalTeam = new Dictionary<EnemyPokemonGeneration, (bool, int)>();//first value isdown,second value currenthp
    //    private PlayerPokemonGeneration playerPokemon {get; set;}
    //    private  IMoveResult playerMove { get; set;}
    //    private MoveData rivalMove {get; set;}
    //    //iBotBattle                             
    //    public int activePokemonHp { get; set; }
    //    public EnemyPokemonGeneration activePokemon { get; set;}
       
    //    //ITranier
    //    public int MoneyReward { get; set; } // How much money the player receives on win
    //    public bool CanRematch { get; set; } // Can this trainer be re-battled?
    //    public bool IsDeafeted { get; set; } // Used to track game progress
    //    public bool IsHidden { get; set; } // Set to true to hide trainer until triggered
    //    public string EncounterLocation { get; set; } // Where this trainer appears
    //    public bool IsBattleMandatory { get; set; } // If true, auto battle starts on encounter
    //    public string MusicTheme { get; set; } // Theme music key
    //    public string SpriteAssetKey { get; set; } // Used to load sprite
    //    public List<string> ItemRewards { get; set; } // Rewards after victory
    //    public string Name { get; set; } // Change to match rival name
    //    public string Description { get; set; }
    //    public List<string> PreBattleDialog { get; set; }
    //    public List<string> PostBattleDialogWin { get; set; }
    //    public List<string> PostBattleDialogLose { get; set; }
    //    public List<string> MidBattleDialog { get; set; }
        
    //    public RivalBot(List<EnemyPokemonGeneration> rivalTeam,PlayerPokemonGeneration playerPokemon)
    //    {
    //        foreach(EnemyPokemonGeneration rivalPokemon in rivalTeam)
    //        {
    //            this.rivalTeam.Add(rivalPokemon, (false, rivalPokemon.MaxHP));
    //        }
    //        this.playerPokemon = playerPokemon;
    //        this.activePokemon = rivalTeam[0];
    //        this.activePokemonHp = this.activePokemon.MaxHP;
    //    }
    //    public int UpdateData(PlayerPokemonGeneration playerPokemon,IMoveResult playermove,int currentHp)
    //    {
    //        this.playerPokemon = playerPokemon;
    //        this.playerMove = playermove;
    //        this.activePokemonHp = currentHp;
    //        this.rivalMove = ChooseMove();
    //        if(this.activePokemon.StatusType == StatusType.None)
    //        {
    //            this.activePokemon.StatusType = this.playerMove.StatusEffect;
    //        }
    //        if (!HasPriority(this.rivalMove,this.playerMove))
    //        {
    //            ReceiveDamage();
                
    //        }
    //        return this.activePokemonHp;
    //    }
    //    public bool HasPriority(MoveData rivalMove, IMoveResult playerMove)
    //    {
    //        if (rivalMove.Priority != playerMove.Priority)
    //            return rivalMove.Priority > playerMove.Priority;

    //        // If equal, fall back to Speed
    //        return this.activePokemon.IVs.Speed > playerPokemon.IVs.Speed;
    //    }

    //    public void ChooseNextPokemon()
    //    {
    //        foreach (var kvp in this.rivalTeam)
    //        {
    //            if (!kvp.Value.Item1) // Item1 = isFainted
    //            {
    //                this.activePokemon = kvp.Key;
    //                this.activePokemonHp = kvp.Value.Item2;
    //                break;
    //            }
    //        }
    //    }
    //    public bool ShouldSwitchPokemon()
    //    {
    //        // Fainted
    //        if (this.rivalTeam[activePokemon].Item1) return true;

    //        // If HP < 25% and there’s a healthy Pokémon with type advantage
    //        if (this.activePokemonHp < this.activePokemon.MaxHP * 0.25)
    //        {
    //            foreach (var kvp in rivalTeam)
    //            {
    //                if (!kvp.Value.Item1 &&
    //                    TypeEffectivenessChartHelper.GetTypeEffectiveness(kvp.Key.Types, this.playerPokemon.Types) > 1.0)
    //                {
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    public void SwitchPokemon()
    //    {
    //        if (ShouldSwitchPokemon())
    //        {
    //            ChooseNextPokemon();
    //        }
    //    }
    //    public MoveData ChooseMove()
    //    {
    //        var availableMoves = this.activePokemon.Moves
    //            .Where(m => m.Value > 0)
    //            .Select(m => m.Key)
    //            .ToList();

    //        if (availableMoves.Count == 0) return null;

    //        // Score moves based on effectiveness and power
    //        return availableMoves
    //            .OrderByDescending(move =>
    //                TypeEffectivenessChartHelper.GetTotalMoveEffectiveness(move.Type, playerPokemon.Types) *
    //                (move.Power + (this.activePokemon.Types.Contains(move.Type) ? 15 : 0)) // STAB bonus
    //            )
    //            .ThenByDescending(move => move.Accuracy)
    //            .First();
    //    }

    //    public int HealPokemon(string item)
    //    {
    //        return activePokemonHp;
    //    }
    //    public MoveResult ExecuteMove()
    //    {
    //        if (!rivalTeam[activePokemon].Item1)
    //        { 
    //            MoveData Movedata = rivalMove;
    //            MoveResult moveResult = BattleCalculator.ExecuteMove(playerPokemon, activePokemon, Movedata);
    //            if (BattleCalculator.DoesMoveHit(Movedata,this.activePokemon.StatusType))
    //            {
    //                this.activePokemon.Moves[Movedata] -= 1;
    //                if (moveResult.IsSwitch)
    //                {
    //                    SwitchPokemon();
    //                }
    //                return moveResult;
    //            }
    //        }
            
    //        return null;
    //    }
    //    public void ReceiveDamage()
    //    {
    //        activePokemonHp -= playerMove.Damage;
    //        if (activePokemonHp <= 0)
    //        {
    //            this.activePokemonHp = 0;
    //            rivalTeam[activePokemon] = (true, 0);
    //        }
    //    }

    //    public int EndTurn(bool HasPriority)
    //    {
    //        ApplyStatusEffects();
    //        if (HasPriority)
    //        {
    //            ReceiveDamage();
    //        }
    //        if (activePokemonHp <= 0)
    //        {
    //            SwitchPokemon();
    //        }
    //        return this.activePokemonHp;
    //    }

    //    private void ApplyStatusEffects()
    //    {
    //        switch (this.activePokemon.StatusType)
    //        {
    //            case StatusType.Burn:
    //            case StatusType.Poison:
    //                this.activePokemonHp -= (int)(this.activePokemon.MaxHP * 0.0625);
    //                break;
    //        }
    //    }

    }
}
