// Server/Hubs/ServerBattleSession.cs

using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Server.Services;
using PokemonGame.Services.Factory;
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

        private string _player1Name = string.Empty;
        private string _player2Name = string.Empty;

        private string _p1Conn = string.Empty;
        private string _p2Conn = string.Empty;
        public string P1ConnectionId => _p1Conn;
        public string P2ConnectionId => _p2Conn;

        private (BattleActionType type, int index)? _p1Action;
        private (BattleActionType type, int index)? _p2Action;

        private bool _p1Registered;
        private bool _p2Registered;

        public bool BothPlayersReady => _p1Registered && _p2Registered;
        public bool BothActionsReady => _p1Action.HasValue && _p2Action.HasValue;

        private readonly ServiceFactory _serviceFactory;

        public ServerBattleSession(string sessionId, ServiceFactory serviceFactory)
        {
            SessionId = sessionId;
            _serviceFactory = serviceFactory;
        }

        public void RegisterPlayer(int playerId, string connectionId, MatchmakingEntry entry)
        {
            if (playerId <= 0)
                throw new InvalidOperationException("Cannot register battle player with id 0.");

            // Existing player 1 reconnect / duplicate connection
            if (_p1Registered && Player1Id == playerId)
            {
                _p1Conn = connectionId;
                _player1Name = entry.PlayerName;
                _p1Entry = entry;
                return;
            }

            // Existing player 2 reconnect / duplicate connection
            if (_p2Registered && Player2Id == playerId)
            {
                _p2Conn = connectionId;
                _player2Name = entry.PlayerName;
                _p2Entry = entry;
                return;
            }

            if (!_p1Registered)
            {
                Player1Id = playerId;
                _player1Name = entry.PlayerName;
                _p1Conn = connectionId;
                _p1Entry = entry;
                _p1Registered = true;
                return;
            }

            if (!_p2Registered)
            {
                Player2Id = playerId;
                _player2Name = entry.PlayerName;
                _p2Conn = connectionId;
                _p2Entry = entry;
                _p2Registered = true;

                if (Manager == null)
                    InitialiseBattle();

                return;
            }

            throw new InvalidOperationException(
                $"Session {SessionId} is already full. Cannot register player {playerId}.");
        }

        public bool IsPlayer1(int playerId) => playerId == Player1Id;
        public bool HasConnection(string id) => _p1Conn == id || _p2Conn == id;
        public int GetPlayerByConnection(string id) => _p1Conn == id ? Player1Id : Player2Id;

        public void RecordAction(int playerId, BattleActionType type, int index)
        {
            if (IsPlayer1(playerId))
                _p1Action = (type, index);
            else
                _p2Action = (type, index);
        }

        public (BattleActionType type, int index) GetAction(int playerId)
        {
            return IsPlayer1(playerId)
                ? _p1Action!.Value
                : _p2Action!.Value;
        }

        public void ClearActions()
        {
            _p1Action = null;
            _p2Action = null;
        }

        // ── FIX #3: call RunTurnPvP so both human move indices are used ───────
        // The old code called Manager.SetOpponentAction() which doesn't exist,
        // then Manager.RunTurn(p1IdxOnly) which would have run the bot AI for P2.
        // RunTurnPvP bypasses the bot AI entirely — both indices come from the
        // recorded actions of the two human players.
        public void RunPvPTurn()
        {
            var (p1Type, p1Idx) = GetAction(Player1Id);
            var (p2Type, p2Idx) = GetAction(Player2Id);

            BattleActionType p1Action =
                p1Type == BattleActionType.Switch
                    ? BattleActionType.Switch
                    : BattleActionType.Move;

            BattleActionType p2Action =
                p2Type == BattleActionType.Switch
                    ? BattleActionType.Switch
                    : BattleActionType.Move;

            Manager.RunTurnPvP(p1Idx, p2Idx, p1Action, p2Action);
        }

        public BattleSnapshot BuildSnapshot(int requestingPlayerId)
        {
            var myActive = IsPlayer1(requestingPlayerId) ? Manager.PlayerActive : Manager.BotActive;
            var theirActive = IsPlayer1(requestingPlayerId) ? Manager.BotActive : Manager.PlayerActive;

            int? winnerPlayerId = null;
            string? winnerName = null;
            if (Manager.Winner != null)
            {
                bool p1Won = Manager.Winner == Manager.PlayerTeam;
                winnerPlayerId = p1Won ? Player1Id : Player2Id;
                winnerName = p1Won ? _player1Name : _player2Name;
            }

            return new BattleSnapshot
            {
                Player = new PokemonSideSnapshot
                {
                    PokedexId = myActive.PokedexId,
                    Name = myActive.Name,
                    Level = myActive.Level,
                    CurrentHP = myActive.CurrentHP,
                    MaxHP = myActive.MaxHP,
                    StatusCondition = myActive.Status.ToString()
                },
                Enemy = new PokemonSideSnapshot
                {
                    PokedexId = theirActive.PokedexId,
                    Name = theirActive.Name,
                    Level = theirActive.Level,
                    CurrentHP = theirActive.CurrentHP,
                    MaxHP = theirActive.MaxHP,
                    StatusCondition = theirActive.Status.ToString()
                },
                PlayerMoves = myActive.Moves
                    .Select((m, i) => new MoveSnapshot
                    {
                        Index = i,
                        Name = (m as MoveState)?.Name ?? "-",
                        Type = (m as MoveState)?.Element.ToString() ?? string.Empty,
                        PP = (m as MoveState)?.PP ?? 0
                    })
                    .ToList(),
                LogEntries = Manager.logger.BattleLog,
                IsOver = IsOver,
                WinnerName = winnerName,
                WinnerPlayerId = winnerPlayerId
            };
        }

        private void InitialiseBattle()
        {
            var pokemonService = _serviceFactory.PokemonService;
            var moveTranslator = new MoveTranslator(_serviceFactory.MoveService);
            var abilityTranslator = new AbilityTranslator(_serviceFactory.AbilityService);
            var itemTranslator = new ItemTranslator(_serviceFactory.ItemService, moveTranslator);
            var translator = new TeamTranslator(
                pokemonService, moveTranslator, abilityTranslator, itemTranslator);

            var p1Team = BuildTeam(translator, _p1Entry);
            var p2Team = BuildTeam(translator, _p2Entry);

            Manager = new BattleManager(p1Team, p2Team, BotLevel.Easy);
        }
        private static PokemonTeam BuildTeam(TeamTranslator translator, MatchmakingEntry entry)
        {
            if (entry.TeamId > 0)
                return translator.LoadTeamByID(entry.PlayerId);

            var pokemonService = new LocalPokemonService();
            var moveTranslator = new MoveTranslator();
            var abilityTranslator = new AbilityTranslator();
            var itemTranslator = new ItemTranslator();
            var localTranslator = new TeamTranslator(
                pokemonService, moveTranslator, abilityTranslator, itemTranslator);

            var results = pokemonService.GenerateRandomTeam(count: 6, level: 50);
            var roster = results.Select(r => localTranslator.TranslateToDomain(r)).ToList();

            while (roster.Count < 6)
                roster.Add(roster[0]);

            return PokemonTeam.Create(roster);
        }
    }
}