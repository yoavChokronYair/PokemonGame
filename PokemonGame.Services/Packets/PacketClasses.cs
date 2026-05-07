using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;

namespace PokemonGame.Services.Packets
{
    public class FindMatchPacket
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public string BattleMode { get; set; } = string.Empty;
        public bool IsOneVOne { get; set; }
        public int TeamId { get; set; }
        public List<BattlePokemonDto> Team { get; set; } = new();
    }
    public class BattlePokemonDto
    {
        public int PokedexId { get; set; }
    }
    public class LoginPacket
    {
        public string Username { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
    }

    public class RegisterPacket
    {
        public string Username { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
    }
    public class LoginResultPacket
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
    }
    public class CreatePlayerPacket
    {
        public string Username { get; set; } = string.Empty;
    }
    public class UpdateSettingPacket
    {
        public string ColumnName { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class SetFavoriteTeamPacket
    {
        public int TeamId { get; set; }
    }
    public class SaveTeamPacket
    {
        public string TeamName { get; set; } = string.Empty;
        public List<BattlerPokemon> Slots { get; set; } = new();
    }

    public class SyncTeamPacket
    {
        public TeamData Team { get; set; } = new();
        public List<BattlerPokemon> Slots { get; set; } = new();
        public DateTime LastModified { get; set; }
    }
    public class SaveBattlePacket
    {
        public List<BattleParticipantData> Participants { get; set; } = new();
    }

    public class SaveBattleResultPacket
    {
        public int BattleId { get; set; }
    }
    public class MatchFoundPacket
    {
        public string SessionId { get; set; } = string.Empty;
        public int OpponentId { get; set; }
        public string OpponentName { get; set; } = string.Empty;
    }

    public class MatchWaitingPacket
    {
        public string Status { get; set; } = "Waiting";
    }
    public class BattleMovePacket
    {
        public string SessionId { get; set; } = string.Empty;
        public int PlayerId { get; set; }
        public int MoveIndex { get; set; }
    }

    public class BattleSwitchPacket
    {
        public string SessionId { get; set; } = string.Empty;
        public int PlayerId { get; set; }
        public int SlotIndex { get; set; }
    }

    public class BattleStatePacket
    {
        public string SessionId { get; set; } = string.Empty;
    }

}
