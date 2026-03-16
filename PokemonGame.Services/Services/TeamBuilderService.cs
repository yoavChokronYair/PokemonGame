using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Repositories.SQLite;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class TeamBuilderService
    {
        private readonly SQLitePokemonRepository _pokemon;
        private readonly SQLiteAbilityRepository _abilities;
        private readonly SQLiteItemRepository _items;
        private readonly SQLiteMoveLearnsetRepository _learnsets;
        private readonly SQLiteMoveRepository _moves;
        private readonly SQLitePokemonStatsRepository _stats;
        private readonly SQLiteTeamRepository _teams;
        private readonly SQLiteTeamMemberRepository _teamMembers;
        private readonly SQLiteBattlerPokemonRepository _battlerPokemon;

        // Move ID → Name lookup, built once on first use
        private Dictionary<int, string>? _moveNameCache;

        public TeamBuilderService()
        {
            var f = ServiceFactory.Instance;
            _pokemon = f.PokemonRepository;
            _abilities = f.AbilityRepository;
            _items = f.ItemRepository;
            _learnsets = f.MoveLearnsetRepository;
            _moves = f.MoveRepository;
            _stats = f.pokemonStatsRepository;
            _teams = f.TeamRepository;
            _teamMembers = f.TeamMemberRepository;
            _battlerPokemon = f.BattlerPokemonRepository;
        }

        // ── Pokémon list for the picker ───────────────────────────────────────────

        /// <summary>
        /// Returns every Pokémon with base stats, ability names, and full learnset
        /// resolved — ready to bind directly to the AllPokemon collection.
        /// </summary>
        public List<PokemonDisplayEntry> GetAllPokemon()
        {
            var moveNames = GetMoveNameCache();
            var result = new List<PokemonDisplayEntry>();

            foreach (var p in _pokemon.GetAllPokemon())
            {
                var baseStats = _stats.GetBaseStats(p.PokedexID);

                var abilities = new List<string>();
                if (p.FirstAbilityID != null) abilities.Add(_abilities.GetAbility(p.FirstAbilityID.Value)?.Name ?? "");
                if (p.SecondAbilityID != null) abilities.Add(_abilities.GetAbility(p.SecondAbilityID.Value)?.Name ?? "");
                if (p.HiddenAbilityID != null) abilities.Add(_abilities.GetAbility(p.HiddenAbilityID.Value)?.Name ?? "");
                abilities.RemoveAll(string.IsNullOrWhiteSpace);

                // Union all learnset sources, resolve IDs to names, sort alphabetically
                var moveIds = new HashSet<int>();
                foreach (var m in _learnsets.GetLevelUpMoves(p.PokedexID)) moveIds.Add(m.MoveID);
                foreach (var m in _learnsets.GetMachineMoves(p.PokedexID)) moveIds.Add(m.MoveID);
                foreach (var m in _learnsets.GetTutorMoves(p.PokedexID)) moveIds.Add(m.MoveID);
                foreach (var m in _learnsets.GetEggMoves(p.PokedexID)) moveIds.Add(m.MoveID);

                var availableMoves = moveIds
                    .Where(id => moveNames.ContainsKey(id))
                    .Select(id => moveNames[id])
                    .OrderBy(n => n)
                    .ToList();

                result.Add(new PokemonDisplayEntry
                {
                    PokedexId = p.PokedexID,
                    Name = p.Name ?? string.Empty,
                    Type1 = p.Type1 ?? string.Empty,
                    Type2 = p.Type2,
                    Abilities = abilities,
                    AvailableMoves = availableMoves,
                    HP = baseStats?.HP ?? 0,
                    Atk = baseStats?.Attack ?? 0,
                    Def = baseStats?.Defense ?? 0,
                    SpA = baseStats?.SpAtk ?? 0,
                    SpD = baseStats?.SpDef ?? 0,
                    Spe = baseStats?.Speed ?? 0,
                });
            }

            return result;
        }

        // ── Item list for the picker ──────────────────────────────────────────────

        /// <summary>
        /// Returns all held items excluding Pokéballs, ready to bind to AllItems.
        /// </summary>
        public List<ItemData> GetHeldItems() =>
            _items.GetAllItems()
                  .Where(i => i.Category != "Pokeball")
                  .ToList();

        // ── Team persistence ──────────────────────────────────────────────────────

        /// <summary>
        /// Loads all teams for a user and resolves each slot's full display data.
        /// </summary>
        public List<TeamData> GetUserTeams(int userId) =>
            _teams.GetUserTeams(userId);

        /// <summary>
        /// Gets the BattlerPokemon instances for every slot in a team.
        /// </summary>
        public List<BattlerPokemon> GetTeamMembers(int teamId)
        {
            var slots = _teamMembers.GetTeamMembers(teamId);
            var result = new List<BattlerPokemon>();

            foreach (var slot in slots)
            {
                var bp = _battlerPokemon.GetPokemonInstance(slot.PokemonID);
                if (bp != null) result.Add(bp);
            }

            return result;
        }

        /// <summary>
        /// Creates a new team, saves each slot as a BattlerPokemon, and links them.
        /// Returns the new TeamData.
        /// </summary>
        public TeamData SaveTeam(string teamName, int userId, List<BattlerPokemon> slots)
        {
            var team = _teams.CreateTeam(teamName, userId);

            for (int i = 0; i < slots.Count && i < 6; i++)
            {
                var pokemonId = _battlerPokemon.CreatePokemonInstance(slots[i]);
                _teamMembers.SetPokemonInSlot(team.Id, pokemonId, i + 1);
            }

            return team;
        }

        /// <summary>
        /// Replaces a single slot in an existing team.
        /// If the slot already had a Pokémon, it is deleted first.
        /// </summary>
        public void ReplaceTeamSlot(int teamId, int slotNumber, BattlerPokemon pokemon)
        {
            // Remove existing occupant of this slot
            var existing = _teamMembers.GetTeamMembers(teamId)
                                       .FirstOrDefault(m => m.Slot_number == slotNumber);
            if (existing != null)
            {
                _battlerPokemon.DeletePokemonInstance(existing.PokemonID);
            }

            var newId = _battlerPokemon.CreatePokemonInstance(pokemon);
            _teamMembers.SetPokemonInSlot(teamId, newId, slotNumber);
        }

        /// <summary>
        /// Removes a single Pokémon slot from a team.
        /// </summary>
        public void RemoveTeamSlot(int teamId, int pokemonId)
        {
            _teamMembers.RemovePokemonFromTeam(teamId, pokemonId);
            _battlerPokemon.DeletePokemonInstance(pokemonId);
        }

        // ── Conversion helper: PokemonEntry → BattlerPokemon ─────────────────────

        /// <summary>
        /// Converts a fully-edited UI PokemonEntry into the BattlerPokemon
        /// data class ready to be persisted.
        /// Resolves move names back to IDs using the learnset cache.
        /// </summary>
        public BattlerPokemon ToBattlerPokemon(PokemonDisplayEntry entry, int abilityId,
                                               int? itemId, int move1Id, int? move2Id,
                                               int? move3Id, int? move4Id)
        {
            return new BattlerPokemon
            {
                PokedexID = entry.PokedexId,
                AbilityID = abilityId,
                ItemID = itemId,
                Shiny = entry.IsShiny ? 1 : 0,
                Gender = entry.Gender,
                Level = entry.Level,
                Move1ID = move1Id,
                Move2ID = move2Id,
                Move3ID = move3Id,
                Move4ID = move4Id,
                Iv_hp = entry.IvHP,
                Iv_atk = entry.IvAtk,
                Iv_def = entry.IvDef,
                Iv_spAtk = entry.IvSpA,
                Iv_spDef = entry.IvSpD,
                Iv_speed = entry.IvSpe,
                Ev_hp = entry.EvHP,
                Ev_atk = entry.EvAtk,
                Ev_def = entry.EvDef,
                Ev_spAtk = entry.EvSpA,
                Ev_spDef = entry.EvSpD,
                Ev_speed = entry.EvSpe,
                Nature = entry.Nature,
            };
        }

        /// <summary>
        /// Looks up a move ID by name. Returns null if not found.
        /// </summary>
        public int? GetMoveId(string? moveName)
        {
            if (string.IsNullOrWhiteSpace(moveName)) return null;
            var cache = GetMoveNameCache();
            var pair = cache.FirstOrDefault(kv => kv.Value == moveName);
            return pair.Key == 0 && pair.Value == null ? null : pair.Key;
        }

        /// <summary>
        /// Looks up an ability ID by name. Returns 0 if not found.
        /// </summary>
        public int GetAbilityId(string? abilityName)
        {
            if (string.IsNullOrWhiteSpace(abilityName)) return 0;
            return _abilities.GetAllAbilities()
                             .FirstOrDefault(a => a.Name == abilityName)?.Id ?? 0;
        }

        /// <summary>
        /// Looks up an item ID by name. Returns null if not found.
        /// </summary>
        public int? GetItemId(string? itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName)) return null;
            return _items.GetAllItems()
                         .FirstOrDefault(i => i.Name == itemName)?.Id;
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private Dictionary<int, string> GetMoveNameCache()
        {
            if (_moveNameCache == null)
            {
                _moveNameCache = _moves.GetAllMoves()
                                       .ToDictionary(m => m.Id, m => m.Name);
            }
            return _moveNameCache;
        }
    }

    // ── Display model returned by GetAllPokemon() ─────────────────────────────────
    // Lives here (service layer) because it is purely a data transfer object —
    // the ViewModel maps it into its own PokemonEntry for UI binding.

    public class PokemonDisplayEntry
    {
        public int PokedexId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type1 { get; set; } = string.Empty;
        public string? Type2 { get; set; }
        public List<string> Abilities { get; set; } = new List<string>();
        public List<string> AvailableMoves { get; set; } = new List<string>();

        // Base stats
        public int HP { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int SpA { get; set; }
        public int SpD { get; set; }
        public int Spe { get; set; }
        public int BST => HP + Atk + Def + SpA + SpD + Spe;

        // Editable — set by VM after the user customizes the entry
        public string Nickname { get; set; } = string.Empty;
        public int Level { get; set; } = 100;
        public string Gender { get; set; } = "—";
        public bool IsShiny { get; set; }
        public string? Nature { get; set; } = "Serious";
        public int EvHP { get; set; }
        public int EvAtk { get; set; }
        public int EvDef { get; set; }
        public int EvSpA { get; set; }
        public int EvSpD { get; set; }
        public int EvSpe { get; set; }
        public int IvHP { get; set; } = 31; public int IvAtk { get; set; } = 31;
        public int IvDef { get; set; } = 31; public int IvSpA { get; set; } = 31;
        public int IvSpD { get; set; } = 31; public int IvSpe { get; set; } = 31;
    }
}