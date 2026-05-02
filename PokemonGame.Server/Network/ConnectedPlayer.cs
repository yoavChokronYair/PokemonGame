// PokemonGame.Server/Network/ConnectedPlayer.cs
// Represents one TCP connection on the server side.
// Wraps the TcpClient with identity info populated after the FindMatchPacket arrives.

using System.Net.Sockets;
using PokemonGame.Services.Network.Packets;

namespace PokemonGame.Server.Network
{
    public class ConnectedPlayer
    {
        public TcpClient TcpClient { get; }
        public NetworkStream Stream => TcpClient.GetStream();

        // Populated once FindMatchPacket is received
        public int PlayerId { get; set; }
        public int TeamId { get; set; }

        public string PlayerName { get; set; } = "Unknown";
        public string BattleMode { get; set; } = "fullTeam";
        public bool IsOneVOne { get; set; }
        public System.Collections.Generic.List<BattlePokemonDto> Team { get; set; } = new();

        // Set by MatchmakingQueue once a room is assigned
        public string? RoomId { get; set; }

        public ConnectedPlayer(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
        }
    }
}