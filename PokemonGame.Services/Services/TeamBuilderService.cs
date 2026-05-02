using PokemonGame.Services.Data.GameData;
using PokemonGame.Services.Data.GameData.OnlineBattleData;
using PokemonGame.Services.Data.GameData.Pokemon;
using PokemonGame.Services.Data.Repositories;
using PokemonGame.Services.Factory;

namespace PokemonGame.Services.Handler
{
    public class TeamBuilderService
    {
        private readonly PokemonRepository _pokemon;
        private readonly AbilityRepository _abilities;
        private readonly ItemRepository _items;
        private readonly MoveLearnsetRepository _learnsets;
        private readonly MoveRepository _moves;
        private readonly AttemptRepository _attempts;
        private readonly EffectRepository _effects;
        private readonly NumberRepository _numbers;
        private readonly SequenceStepRepository _sequenceSteps;
        private readonly PokemonStatsRepository _stats;
        private readonly TeamRepository _teams;
        private readonly TeamMemberRepository _teamMembers;
        private readonly BattlerPokemonRepository _battlerPokemon;

        private Dictionary<int, string>? _moveNameCache;
        private Dictionary<int, MoveDisplayEntry>? _moveDisplayCache;
        internal TeamBuilderService(
            PokemonRepository pokemon, AbilityRepository abilities, ItemRepository items,
            MoveLearnsetRepository learnsets, MoveRepository moves, AttemptRepository attempts,
            EffectRepository effects, NumberRepository numbers, SequenceStepRepository sequenceSteps,
            PokemonStatsRepository stats, TeamRepository teams, TeamMemberRepository teamMembers,
            BattlerPokemonRepository battlerPokemon)
        {
            _pokemon = pokemon;
            _abilities = abilities;
            _items = items;
            _learnsets = learnsets;
            _moves = moves;
            _attempts = attempts;
            _effects = effects;
            _numbers = numbers;
            _sequenceSteps = sequenceSteps;
            _stats = stats;
            _teams = teams;
            _teamMembers = teamMembers;
            _battlerPokemon = battlerPokemon;
        }
        public TeamBuilderService()
        {
            var f = ServiceFactory.Instance;
            _pokemon = f.PokemonRepository;
            _abilities = f.AbilityRepository;
            _items = f.ItemRepository;
            _learnsets = f.MoveLearnsetRepository;
            _moves = f.MoveRepository;
            _attempts = f.AttemptRepository;
            _effects = f.EffectRepository;
            _numbers = f.NumberRepository;
            _sequenceSteps = f.SequenceStepRepository;
            _stats = f.PokemonStatsRepository;
            _teams = f.TeamRepository;
            _teamMembers = f.TeamMemberRepository;
            _battlerPokemon = f.BattlerPokemonRepository;
        }

        // ── Team helpers ──────────────────────────────────────────────────────
        public void DeleteTeamsByPlayer(int battlePlayerId) =>
    _teams.DeleteTeamsByPlayer(battlePlayerId);
        public TeamData? GetTeamByBattlePlayer(int battlePlayerId) =>
            _teams.GetTeamByBattlePlayer(battlePlayerId);

        public List<TeamData> GetTeamsByBattlePlayer(int battlePlayerId) =>
            _teams.GetTeamsByBattlePlayer(battlePlayerId);

        public bool CanCreateTeam(int battlePlayerId) =>
            _teams.CanCreateTeam(battlePlayerId);

        public void DeleteTeam(int teamId) =>
            _teams.DeleteTeam(teamId);

        // ── Pokémon list for the picker ───────────────────────────────────────

        public List<PokemonDisplayEntry> GetAllPokemon()
        {
            var moveEntries = GetMoveDisplayCache();
            var allPokemon = _pokemon.GetAllPokemon();
            var result = new List<PokemonDisplayEntry>();

            foreach (var p in allPokemon)
            {
                var baseStats = _stats.GetBaseStats(p.PokedexID);

                var abilities = new List<string>();
                if (p.FirstAbilityID != null)
                {
                    abilities.Add(_abilities.GetAbilityById(p.FirstAbilityID.Value)?.Name ?? "");
                }

                if (p.SecondAbilityID != null)
                {
                    abilities.Add(_abilities.GetAbilityById(p.SecondAbilityID.Value)?.Name ?? "");
                }

                if (p.HiddenAbilityID != null)
                {
                    abilities.Add(_abilities.GetAbilityById(p.HiddenAbilityID.Value)?.Name ?? "");
                }

                abilities.RemoveAll(string.IsNullOrWhiteSpace);

                var moveIds = new HashSet<int>();

                void AddLearnset(int dexId)
                {
                    foreach (var m in _learnsets.GetLevelUpMoves(dexId))
                    {
                        moveIds.Add(m.MoveID);
                    }

                    foreach (var m in _learnsets.GetMachineMoves(dexId))
                    {
                        moveIds.Add(m.MoveID);
                    }

                    foreach (var m in _learnsets.GetTutorMoves(dexId))
                    {
                        moveIds.Add(m.MoveID);
                    }

                    foreach (var m in _learnsets.GetEggMoves(dexId))
                    {
                        moveIds.Add(m.MoveID);
                    }
                }

                AddLearnset(p.PokedexID);

                // Walk pre-evolution chain
                var visited = new HashSet<int> { p.PokedexID };
                var current = p;
                while (true)
                {
                    var preEvo = allPokemon.FirstOrDefault(x => x.PokemonEvoID == current.PokedexID);
                    if (preEvo == null || visited.Contains(preEvo.PokedexID))
                    {
                        break;
                    }

                    visited.Add(preEvo.PokedexID);
                    AddLearnset(preEvo.PokedexID);
                    current = preEvo;
                }

                var availableMoves = moveIds
                    .Where(id => moveEntries.ContainsKey(id))
                    .Select(id => moveEntries[id])
                    .OrderBy(m => m.Name)
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
                    Types = new List<string> { p.Type1, p.Type2 }
                                         .Where(t => !string.IsNullOrEmpty(t)).ToList(),
                });
            }

            return result;
        }

        // ── Item list ─────────────────────────────────────────────────────────

        public List<ItemData> GetHeldItems() =>
           _items.GetAllHeldItems();

        // ── Team members ──────────────────────────────────────────────────────

        public List<BattlerPokemon> GetTeamMembers(int teamId)
        {
            var result = new List<BattlerPokemon>();
            foreach (var slot in _teamMembers.GetTeamMembers(teamId))
            {
                var bp = _battlerPokemon.GetPokemonInstance(slot.PokemonID);
                if (bp != null)
                {
                    result.Add(bp);
                }
            }
            return result;
        }

        public TeamData SaveTeam(string teamName, int battlePlayerId, List<BattlerPokemon> slots)
        {
            var team = _teams.CreateTeam(teamName, battlePlayerId);
            for (int i = 0; i < slots.Count && i < 6; i++)
            {
                var pokemonId = _battlerPokemon.CreatePokemonInstance(slots[i]);
                if (pokemonId <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Slot {i + 1} failed — pokemonId={pokemonId}");
                    continue;
                }
                _teamMembers.SetPokemonInSlot(team.Id, pokemonId, i + 1);
            }
            return team;
        }

        public void ReplaceTeamSlot(int teamId, int slotNumber, BattlerPokemon pokemon)
        {
            var existing = _teamMembers.GetTeamMembers(teamId)
                                       .FirstOrDefault(m => m.Slot_number == slotNumber);
            if (existing != null)
            {
                _battlerPokemon.DeletePokemonInstance(existing.PokemonID);
            }

            var newId = _battlerPokemon.CreatePokemonInstance(pokemon);
            _teamMembers.SetPokemonInSlot(teamId, newId, slotNumber);
        }

        public void UpdateTeam(int teamId, string teamName, List<BattlerPokemon> slots)
        {
            _teams.UpdateTeamName(teamId, teamName);

            foreach (var m in _teamMembers.GetTeamMembers(teamId))
            {
                _battlerPokemon.DeletePokemonInstance(m.PokemonID);
                _teamMembers.RemovePokemonFromTeam(teamId, m.PokemonID);
            }

            for (int i = 0; i < slots.Count && i < 6; i++)
            {
                try
                {
                    var pokemonId = _battlerPokemon.CreatePokemonInstance(slots[i]);
                    if (pokemonId <= 0)
                    {
                        continue;
                    }

                    _teamMembers.SetPokemonInSlot(teamId, pokemonId, i + 1);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Slot {i + 1} failed: {ex.Message}");
                }
            }
        }

        public void RemoveTeamSlot(int teamId, int pokemonId)
        {
            _teamMembers.RemovePokemonFromTeam(teamId, pokemonId);
            _battlerPokemon.DeletePokemonInstance(pokemonId);
        }

        // ── Conversion helpers ────────────────────────────────────────────────

        public BattlerPokemon ToBattlerPokemon(PokemonDisplayEntry entry, int abilityId,
                                               int? itemId, int move1Id, int? move2Id,
                                               int? move3Id, int? move4Id) =>
            new BattlerPokemon
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

        public int? GetMoveId(string? moveName)
        {
            if (string.IsNullOrWhiteSpace(moveName))
            {
                return null;
            }

            var cache = GetMoveDisplayCache();
            var pair = cache.FirstOrDefault(kv => kv.Value.Name == moveName);
            return pair.Value == null ? null : (int?)pair.Key;
        }

        public MoveDisplayEntry GetMoveById(int? moveId, List<MoveDisplayEntry> availableMoves)
        {
            if (moveId == null || moveId <= 0)
            {
                return null;
            }

            return availableMoves.FirstOrDefault(m => m.Id == moveId);
        }

        public string GetAbilityNameById(int abilityId) =>
            _abilities.GetAbilityById(abilityId)?.Name ?? string.Empty;

        public int GetAbilityId(string? abilityName)
        {
            if (string.IsNullOrWhiteSpace(abilityName))
            {
                return 0;
            }

            return _abilities.GetAllAbilities().FirstOrDefault(a => a.Name == abilityName)?.Id ?? 0;
        }

        public int? GetItemId(string? itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return null;
            }

            return _items.GetAllItems().FirstOrDefault(i => i.Name == itemName)?.Id;
        }

        // ── Private cache builders ────────────────────────────────────────────

        private Dictionary<int, MoveDisplayEntry> GetMoveDisplayCache()
        {
            if (_moveDisplayCache != null)
            {
                return _moveDisplayCache;
            }

            _moveDisplayCache = new Dictionary<int, MoveDisplayEntry>();

            foreach (var m in _moves.GetAll())
            {
                double? accuracy = null;
                int? power = null;

                var firstAttempt = _attempts.LoadForMove(m.Id).FirstOrDefault();

                if (firstAttempt?.AccuracyValue.HasValue == true)
                {
                    accuracy = firstAttempt.AccuracyValue.Value;
                }

                if (firstAttempt?.OnHitEffectId.HasValue == true)
                {
                    var onHit = _effects.Load(firstAttempt.OnHitEffectId.Value);
                    if (onHit != null)
                    {
                        if (onHit.Type == "FormulaDamage" && onHit.NumberId.HasValue)
                        {
                            var number = _numbers.Load(onHit.NumberId.Value);
                            if (number?.ExactValue.HasValue == true)
                            {
                                power = (int)number.ExactValue.Value;
                            }
                        }
                        else if (onHit.Type == "Sequence")
                        {
                            foreach (var step in _sequenceSteps.LoadForEffect(onHit.Id))
                            {
                                var stepEffect = _effects.Load(step.ChildEffectId);
                                if (stepEffect?.Type == "FormulaDamage" && stepEffect.NumberId.HasValue)
                                {
                                    var number = _numbers.Load(stepEffect.NumberId.Value);
                                    if (number?.ExactValue.HasValue == true)
                                    {
                                        power = (int)number.ExactValue.Value;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (onHit.Type == "Chance" && onHit.ChildEffectId.HasValue)
                        {
                            var child = _effects.Load(onHit.ChildEffectId.Value);
                            if (child?.Type == "FormulaDamage" && child.NumberId.HasValue)
                            {
                                var number = _numbers.Load(child.NumberId.Value);
                                if (number?.ExactValue.HasValue == true)
                                {
                                    power = (int)number.ExactValue.Value;
                                }
                            }
                        }
                    }
                }

                _moveDisplayCache[m.Id] = new MoveDisplayEntry
                {
                    Id = m.Id,
                    Name = m.Name ?? string.Empty,
                    TypeName = m.Element ?? string.Empty,
                    Category = m.Category ?? string.Empty,
                    Power = power,
                    Accuracy = accuracy.HasValue ? (int)(accuracy.Value * 100) : (int?)null,
                    PP = m.PP,
                    Description = m.Description ?? string.Empty,
                };
            }

            return _moveDisplayCache;
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    public class MoveDisplayEntry
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int? Power { get; set; }
        public int? Accuracy { get; set; }
        public int PP { get; set; }
        public string Description { get; set; } = string.Empty;

        public string PowerDisplay => Power.HasValue ? Power.Value.ToString() : "—";
        public string AccuracyDisplay => Accuracy.HasValue ? $"{Accuracy.Value}%" : "—";
    }

    public class PokemonDisplayEntry
    {
        public int PokedexId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type1 { get; set; } = string.Empty;
        public string? Type2 { get; set; }
        public List<string> Abilities { get; set; } = new();
        public List<MoveDisplayEntry> AvailableMoves { get; set; } = new();
        public List<string> Types { get; set; } = new();
        public string AbilityPrimary => Abilities.Count > 0 ? Abilities[0] : string.Empty;
        public string AbilityHidden => Abilities.Count > 2 ? Abilities[2] : string.Empty;

        public int HP { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int SpA { get; set; }
        public int SpD { get; set; }
        public int Spe { get; set; }
        public int BST => HP + Atk + Def + SpA + SpD + Spe;

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