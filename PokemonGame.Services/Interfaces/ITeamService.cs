using System;
using System.Collections.Generic;
using System.Text;
using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Handler;

namespace PokemonGame.Services.Interfaces
{
    public interface ITeamService
    {
        List<TeamData> GetTeamsByBattlePlayer(int battlePlayerId);
        TeamData? GetTeamByBattlePlayer(int battlePlayerId);
        bool CanCreateTeam(int battlePlayerId);
        void DeleteTeam(int teamId);
        List<BattlerPokemon> GetTeamMembers(int teamId);
        TeamData SaveTeam(string teamName, int battlePlayerId, List<BattlerPokemon> slots);
        void UpdateTeam(int teamId, string teamName, List<BattlerPokemon> slots);
        void ReplaceTeamSlot(int teamId, int slotNumber, BattlerPokemon pokemon);
        void RemoveTeamSlot(int teamId, int pokemonId);
    }

    // ── Pokédex data — used by the pokemon picker UI ──────────────────────────
    public interface IPokedexService
    {
        List<PokemonDisplayEntry> GetAllPokemon();
        List<ItemData> GetHeldItems();
        int GetAbilityId(string? abilityName);
        string GetAbilityNameById(int abilityId);
        int? GetItemId(string? itemName);
        int? GetMoveId(string? moveName);
        MoveDisplayEntry? GetMoveById(int? moveId, List<MoveDisplayEntry> availableMoves);
        BattlerPokemon ToBattlerPokemon(PokemonDisplayEntry entry, int abilityId,
                                        int? itemId, int move1Id, int? move2Id,
                                        int? move3Id, int? move4Id);
    }
}
