// PokemonGame.Server/BattleRoom/TeamBuilder.cs
// Converts the FindMatchPacket DTO list into a full PokemonTeam.
// The server loads complete stats from its own DB via TeamTranslator.
// Falls back to DTO values only if the DB lookup fails.

using PokemonGame.Core.Config;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Model.Managers;
using PokemonGame.Services.Network.Packets;

namespace PokemonGame.Server.BattleRoom
{
    public static class TeamBuilder
    {
        /// <summary>
        /// Build a full <see cref="PokemonTeam"/> for a connected player.
        /// Loads complete data (stats, moves, ability, item) from the server DB
        /// using the player's BattlePlayerID. Falls back to DTO stubs if the DB
        /// lookup returns nothing.
        /// </summary>
        public static PokemonTeam BuildFromPlayer(int battlePlayerId,
                                                   List<BattlePokemonDto> dtos)
        {
            try
            {
                // TeamTranslator lives in PokemonGame.ViewModels and talks to
                // PokemonGame.Services — both are referenced by the server project.
                var translator = new TeamTranslator();
                return translator.LoadTeamByID(battlePlayerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TeamBuilder] DB load failed for player {battlePlayerId}: {ex.Message}. Using DTO stubs.");
                return BuildFromDtos(dtos);
            }
        }

        /// <summary>
        /// Fallback: build a minimal team from the compact DTOs that arrived
        /// over the wire. Stats will be incomplete but the game can still run.
        /// </summary>
        public static PokemonTeam BuildFromDtos(List<BattlePokemonDto> dtos)
        {
            var states = new List<PokemonState>();

            foreach (var dto in dtos)
            {
                states.Add(new PokemonState
                {
                    Name = dto.Name,
                    PokedexId = dto.PokedexId,
                    Level = dto.Level,
                    CurrentHP = dto.Hp,
                    MaxHP = dto.MaxHp,
                });
            }

            // PokemonTeam.Create() requires exactly PokemonConstants.PartyCapacity (6) slots.
            while (states.Count < PokemonConstants.PartyCapacity)
            {
                // IsFainted is computed as CurrentHP <= 0, so HP=0 keeps this slot out of battle.
                states.Add(new PokemonState { Name = "Empty", CurrentHP = 0, MaxHP = 1 });
            }

            return PokemonTeam.Create(states);
        }
    }
}