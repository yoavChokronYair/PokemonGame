using PokemonGame.Interface;
using PokemonGame.Model.Data;
using PokemonGame.Model.Helper;
using PokemonGame.Model.PokemonCreation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonGame.Model.BattleSystem.Bot
{
    public class RivalBot : IBotBattle, ITrainer
    {
        // Tracks which Pokémon is currently active in battle
        private int _activePokemonIndex = 0;

        // Constructor - recreceives a preset team
        public RivalBot(List<EnemyPokemonGeneration> team)
        {
            RivalTeam = team;
            IsFainted = new List<bool>(new bool[team.Count]); // All Pokémon start alive
        }

        // ----------------------------
        // IBotBattle Implementation
        // ----------------------------

        // Current HP of the active Pokémon
        public int ActivePokemonHp => ActivePokemon.CurrentHp;

        // Full team of enemy Pokémon
        public List<EnemyPokemonGeneration> RivalTeam { get; private set; }

        // The current Pokémon in battle
        public EnemyPokemonGeneration ActivePokemon => RivalTeam[_activePokemonIndex];

        // Tracks if each Pokémon has fainted
        public List<bool> IsFainted { get; private set; }

        // Updates the IsFainted list based on current HP
        public void updateData()
        {
            //Todo:add A proper update
            for (int i = 0; i < RivalTeam.Count; i++)
            {
                IsFainted[i] = RivalTeam[i].CurrentHp <= 0;  
            }
            
        }

        // Choose the next available (not fainted) Pokémon to send out
        public void ChooseNextPokemon()
        {
            for (int i = 0; i < RivalTeam.Count; i++)
            {
                if (!IsFainted[i])
                {
                    _activePokemonIndex = i;
                    break;
                }
            }
        }

        // AI logic to choose a move to use on the player’s Pokémon
        public MoveData ChooseMove()
        {
            List<MoveData> availableMoves = new List<MoveData>();
            foreach (var e in ActivePokemon.Moves)
            {
                if(e.Value > 0)
                {
                    availableMoves.Add(e.Key);
                }
            }
            if (availableMoves.Count == 0)
                return null;

            // Simple logic: choose the move with the highest power
            return availableMoves.OrderBy(m => m.Power).FirstOrDefault();
        }

        // Called at the end of the battle to update defeat state
        public void OnBattleEnd(bool won)
        {
            IsDeafeted = !won; // Set flag if the bot lost
        }

        // Heal the current Pokémon (example use: Full Restore)
        public void HealPokemon(string item)
        {
            ActivePokemon.CurrentHp = ActivePokemon.MaxHP;
            foreach (var move in ActivePokemon.Moves)
                move.Key.PP = move.Value;
        }

        // Manually switch to the next available Pokémon
        public void SwitchPokemon()
        {
            ChooseNextPokemon();
        }

        // Decides if the bot should switch out (very basic logic)
        public bool ShouldSwitchPokemon(PlayerPokemonGeneration playerPokemon)
        {
            //Todo:make a ShouldSwitchS
            return false;
        }

        // Determines if this bot should move first (priority)
        public bool HasProirerty(PlayerPokemonGeneration playerPokemonGenaration)
        {
            return ActivePokemon.IVs.Speed > playerPokemonGenaration.IVs.Speed; // Customize this threshold
        }

        // Executes the chosen move and applies damage to the player's Pokémon
        public void ExecuteMove(PlayerPokemonGeneration playerPokemon)
        {
            //ToDo: make a proper one for the ai 
            MoveData moveData = ChooseMove();
            if (moveData == null) return;
            playerPokemon.CurrentHp -= 10;
        }

        // ----------------------------
        // IBattleParticipant Implementation
        // ----------------------------

        // Receive damage from an opponent move
        public void ReceiveDamage(PlayerPokemonGeneration playerPokemon,MoveData move)
        {
            Console.WriteLine($"{ActivePokemon.Nickname} used {move.ename}!");
            ActivePokemon.CurrentHp -= BattleCalculator.ExecuteMove(ActivePokemon, playerPokemon, move).Damage; // Simple damage logic
            move.PP--;
        }

        //ToDo: Apply a status effect (e.g., paralysis, burn)
        public void ApplyStatusEffect(string effect)
        {

        }

        // ----------------------------
        // ITrainer Implementation
        // ----------------------------

        public string Name => "Blue"; // Change to match rival name

        public string Description => "Your childhood rival, cocky but skilled.";


        public List<string> PreBattleDialog => new List<string>
        {
            "Smell ya later!",
            "You're going down!"
        };

        public List<string> PostBattleDialogWin => new List<string>
        {
            "Ugh! You got lucky!",
            "Next time, I’ll destroy you!"
        };

        public List<string> PostBattleDialogLose => new List<string>
        {
            "Hah! You never stood a chance.",
            "Go train harder!"
        };

        public List<string> MidBattleDialog => new List<string>
        {
            "What?! That actually hurt...",
            "You're better than I expected!"
        };

        public int MoneyReward => 1200; // How much money the player receives on win

        public bool CanRematch => true; // Can this trainer be re-battled?

        public bool IsDeafeted { get; private set; } = false; // Used to track game progress

        public bool IsHidden => false; // Set to true to hide trainer until triggered

        public string EncounterLocation => "Route 1"; // Where this trainer appears

        public bool IsBattleMandatory => true; // If true, auto battle starts on encounter

        public string MusicTheme => "rival_theme"; // Theme music key

        public string SpriteAssetKey => "rival_blue_sprite"; // Used to load sprite

        public List<string> ItemRewards => new List<string> { "TM27" }; // Rewards after victory
    }
}
