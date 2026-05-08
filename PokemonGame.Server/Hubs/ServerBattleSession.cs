// Server/Hubs/ServerBattleSession.cs

using Microsoft.AspNetCore.Hosting.Server;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Interface;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Server.Services;
using PokemonGame.Services.Handler;
using PokemonGame.Services.Interfaces;

namespace PokemonGame.Server.Hubs
{
    public class ServerBattleSession
    {
        private MatchmakingEntry _p1Entry = new();
        private MatchmakingEntry _p2Entry = new();
        public string SessionId { get; }
        public BattleManager Manager { get; private set; } = null!;
        public bool IsOver => Manager?.Winner != null;

        public int Player1Id { get; private set; }
        public int Player2Id { get; private set; }

        private string _p1Conn = string.Empty;
        private string _p2Conn = string.Empty;

        private (string type, int index)? _p1Action;
        private (string type, int index)? _p2Action;

        private bool _p1Registered;
        private bool _p2Registered;

        public bool BothPlayersReady => _p1Registered && _p2Registered;
        public bool BothActionsReady => _p1Action.HasValue && _p2Action.HasValue;

        public ServerBattleSession(string sessionId) => SessionId = sessionId;

        public void RegisterPlayer(int playerId, string connectionId, MatchmakingEntry entry)
        {
            if (!_p1Registered)
            {
                Player1Id = playerId;
                _p1Conn = connectionId;
                _p1Entry = entry;
                _p1Registered = true;
            }
            else
            {
                Player2Id = playerId;
                _p2Conn = connectionId;
                _p2Entry = entry;
                _p2Registered = true;

                // Now that we have both entries (and both teams), we can start the manager
                InitialiseBattle();
            }
        }

        public bool IsPlayer1(int playerId) => playerId == Player1Id;
        public bool HasConnection(string id) => _p1Conn == id || _p2Conn == id;
        public int GetPlayerByConnection(string id) => _p1Conn == id ? Player1Id : Player2Id;

        public void RecordAction(int playerId, string type, int index)
        {
            if (IsPlayer1(playerId)) _p1Action = (type, index);
            else _p2Action = (type, index);
        }

        public (string type, int index) GetAction(int playerId) =>
            IsPlayer1(playerId) ? _p1Action!.Value : _p2Action!.Value;

        public void ClearActions()
        {
            _p1Action = null;
            _p2Action = null;
        }

        public BattleSnapshot BuildSnapshot()
        {
            var p = Manager.PlayerActive;
            var e = Manager.BotActive;

            return new BattleSnapshot
            {
                Player = new PokemonSideSnapshot
                {
                    PokedexId = p.PokedexId,
                    Name = p.Name,
                    Level = p.Level,
                    CurrentHP = p.CurrentHP,
                    MaxHP = p.MaxHP,
                    StatusCondition = p.Status.ToString()
                },
                Enemy = new PokemonSideSnapshot
                {
                    PokedexId = e.PokedexId,
                    Name = e.Name,
                    Level = e.Level,
                    CurrentHP = e.CurrentHP,
                    MaxHP = e.MaxHP,
                    StatusCondition = e.Status.ToString()
                },

                // ── translate IMove → MoveSnapshot — no IMove leaks out ───
                PlayerMoves = p.Moves
                    .Select((m, i) => new MoveSnapshot
                    {
                        Index = i,
                        Name = (m as PokemonGame.Model.Domain.Move.MoveState)?.Name ?? "-",
                        Type = (m as PokemonGame.Model.Domain.Move.MoveState)?.Element.ToString() ?? string.Empty,
                        PP = (m as PokemonGame.Model.Domain.Move.MoveState)?.PP ?? 0
                    })
                    .ToList(),

                // Use .BattleLog instead of the logger object itself
                LogEntries = Manager.logger.BattleLog,       
                IsOver = IsOver,
                WinnerName = Manager.Winner?.Active.Name
            };
        }

        private void InitialiseBattle()
        {
            var pokemonService = new LocalPokemonService();
            var moveTranslator = new MoveTranslator();
            var abilityTranslator = new AbilityTranslator();
            var itemTranslator = new ItemTranslator();
            var translator = new TeamTranslator(
                pokemonService,
                moveTranslator,
                abilityTranslator,
                itemTranslator);

            var p1Team = BuildTeam(translator, _p1Entry);
            var p2Team = BuildTeam(translator, _p2Entry);

            Manager = new BattleManager(p1Team, p2Team, BotLevel.Easy);
        }

        private static PokemonTeam BuildTeam(TeamTranslator translator, MatchmakingEntry entry)
        {
            if (entry.TeamId > 0)
                return translator.LoadTeamByID(entry.TeamId);

            // No team ID — generate random
            var pokemonService = new LocalPokemonService();
            var moveTranslator = new MoveTranslator();
            var abilityTranslator = new AbilityTranslator();
            var itemTranslator = new ItemTranslator();
            var localTranslator = new TeamTranslator(
                pokemonService,
                moveTranslator,
                abilityTranslator,
                itemTranslator);

            var results = pokemonService.GenerateRandomTeam(count: 6, level: 50);
            var roster = results
                .Select(r => localTranslator.TranslateToDomain(r))
                .ToList();

            while (roster.Count < 6)
                roster.Add(roster[0]);

            return PokemonTeam.Create(roster);
        }
    }
}