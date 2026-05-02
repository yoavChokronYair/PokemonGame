// PokemonGame.Services/Network/Packets/Packets.cs
// Shared packet definitions for TCP battle communication.
// Both client (ViewModels) and server reference this project, so packets live here.

using System.Collections.Generic;

namespace PokemonGame.Services.Network.Packets
{
    // ── Client → Server ──────────────────────────────────────────────────────

    /// <summary>Sent once the player clicks "Find Match".</summary>
    public class FindMatchPacket
    {
        public string Type { get; set; } = "FindMatch";
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string BattleMode { get; set; } = string.Empty; // "halfTeam" | "TwoThirdsTeam" | "fullTeam"
        public bool IsOneVOne { get; set; }
        public int TeamId { get; set; }

        /// <summary>Minimal team snapshot sent to the server so the BattleRoom can build PokemonState objects.</summary>
        public List<BattlePokemonDto> Team { get; set; } = new();
    }

    /// <summary>Player chose a move during their turn.</summary>
    public class MoveActionPacket
    {
        public string Type { get; set; } = "MoveAction";
        public string RoomId { get; set; } = string.Empty;
        public int MoveIndex { get; set; }
    }

    /// <summary>Player switched Pokémon (forced or voluntary).</summary>
    public class SwitchActionPacket
    {
        public string Type { get; set; } = "SwitchAction";
        public string RoomId { get; set; } = string.Empty;
        public int SlotIndex { get; set; }
    }

    /// <summary>Player disconnected / rage-quit.</summary>
    public class ForfeitPacket
    {
        public string Type { get; set; } = "Forfeit";
        public string RoomId { get; set; } = string.Empty;
    }

    // ── Server → Client ──────────────────────────────────────────────────────

    /// <summary>Opponent found — sent to both players.</summary>
    public class MatchFoundPacket
    {
        public string Type { get; set; } = "MatchFound";
        public string RoomId { get; set; } = string.Empty;
        public string RivalName { get; set; } = string.Empty;
        public int RivalTeamId { get; set; }
        public List<BattlePokemonDto> RivalTeam { get; set; } = new();
        public List<string> PlayerMoveNames { get; set; } = new(); // ADD
        public int PlayerMaxHp { get; set; }                       // ADD
        public int EnemyMaxHp { get; set; }                        // ADD
        public int PlayerPokedexId { get; set; }                   // ADD
        public int EnemyPokedexId { get; set; }                    // ADD
        public int PlayerLevel { get; set; }                       // ADD
        public int EnemyLevel { get; set; }                        // ADD
    }

    /// <summary>Turn result broadcast after BattleManager.RunTurn().</summary>
    public class TurnResultPacket
    {
        public string Type { get; set; } = "TurnResult";
        public string RoomId { get; set; } = string.Empty;
        public int PlayerHp { get; set; }
        public int EnemyHp { get; set; }
        public List<string> LogLines { get; set; } = new();
        public bool PlayerFainted { get; set; }
        public bool EnemyFainted { get; set; }
        public string PlayerStatusCondition { get; set; } = "None"; // ADD
        public string EnemyStatusCondition { get; set; } = "None";  // ADD
        public int PlayerPokedexId { get; set; }                    // ADD (for sprite update on switch)
        public int EnemyPokedexId { get; set; }                     // ADD
    }

    /// <summary>Sent to the player whose active Pokémon fainted — they must choose a replacement.</summary>
    public class ForcedSwitchPacket
    {
        public string Type { get; set; } = "ForcedSwitch";
        public string RoomId { get; set; } = string.Empty;
        /// <summary>Slot indices of still-alive Pokémon the player can switch to.</summary>
        public List<int> AvailableSlots { get; set; } = new();
    }

    /// <summary>Final packet — game over.</summary>
    public class BattleEndPacket
    {
        public string Type { get; set; } = "BattleEnd";
        public string RoomId { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
        public string LoserName { get; set; } = string.Empty;
        public int OpponentBattlePlayerId { get; set; } // ADD THIS
    }

    // ── Shared DTO ───────────────────────────────────────────────────────────

    /// <summary>Lightweight Pokémon snapshot transferred over the wire.</summary>
    public class BattlePokemonDto
    {
        public int PokedexId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public List<string> MoveNames { get; set; } = new();
    }
}