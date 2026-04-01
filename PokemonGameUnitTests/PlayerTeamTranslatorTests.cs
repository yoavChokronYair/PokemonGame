using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Helper.MoveHelper;
using Xunit.Abstractions;

namespace PokemonGame.Tests
{
    // ═════════════════════════════════════════════════════════════════════════
    //  TeamTranslator — Player team (Charizard × 6, HyperBeam + Thunderbolt)
    // ═════════════════════════════════════════════════════════════════════════
    public class PlayerTeamTranslatorTests
    {
        private readonly PokemonTeam _team = BattleTestFactory.PlayerTeam();
        private readonly ITestOutputHelper _out;

        public PlayerTeamTranslatorTests(ITestOutputHelper output)
        {
            _out = output;
        }

        // ── Dump helpers ──────────────────────────────────────────────────────

        private void DumpTeam(PokemonTeam team, string label = "PokemonTeam")
        {
            _out.WriteLine($"══ {label} ════════════════════════════════════");
            _out.WriteLine($"  IsDefeated        : {team.IsDefeated}");
            _out.WriteLine($"  ActiveIndex       : {team.ActiveIndex}");
            _out.WriteLine($"  AlivePokemonCount : {team.GetAlivePokemonCount()}");
            _out.WriteLine($"  TotalPokemon      : {team.getAllPokemonCount()}");
            _out.WriteLine($"  SwitchableSlots   : [{string.Join(", ", team.GetSwitchableIndices())}]");
            _out.WriteLine($"  Active → {team.Active.Name}  HP:{team.Active.CurrentHP}/{team.Active.MaxHP}  Fainted:{team.Active.IsFainted}");
            DumpMoves(team.Active);
            _out.WriteLine("══════════════════════════════════════════════");
        }

        private void DumpMoves(Model.Model.Helper.PokemonHelper.PokemonState p)
        {
            _out.WriteLine($"  Moves ({p.Moves.Count}):");
            for (int i = 0; i < p.Moves.Count; i++)
            {
                var m = p.Moves[i] as MoveState;
                if (m != null)
                {
                    _out.WriteLine($"    [{i}] Name     : {m.Name}");
                    _out.WriteLine($"        Type     : {m.Element}");
                    _out.WriteLine($"        Category : {m.Category}");
                    _out.WriteLine($"        PP       : {m.PP}/{m.MaxPP}");
                    _out.WriteLine($"        Priority : {m.Priority}");
                    _out.WriteLine($"        Target   : {m.Target}");
                }
                else
                {
                    _out.WriteLine($"    [{i}] (non-MoveState: {p.Moves[i].GetType().Name})");
                }
            }
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public void PlayerTeam_IsNotNull()
        {
            DumpTeam(_team, "Player Team");
            Assert.NotNull(_team);
        }

        [Fact]
        public void PlayerTeam_ActivePokemonIsCharizard()
        {
            DumpTeam(_team, "Player Team");
            Assert.Equal("Charizard", _team.Active.Name);
        }

        [Fact]
        public void PlayerTeam_ActivePokemonHasTwoMoves()
        {
            DumpTeam(_team, "Player Team");
            Assert.Equal(2, _team.Active.Moves.Count);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_FirstMoveIsHyperBeam()
        {
            DumpTeam(_team, "Player Team");
            var move = _team.Active.Moves[0] as MoveState;
            _out.WriteLine($"  Moves[0] cast to MoveState: {move?.Name ?? "null"}");
            Assert.NotNull(move);
            Assert.Equal("HyperBeam", move!.Name);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_SecondMoveIsThunderbolt()
        {
            DumpTeam(_team, "Player Team");
            var move = _team.Active.Moves[1] as MoveState;
            _out.WriteLine($"  Moves[1] cast to MoveState: {move?.Name ?? "null"}");
            Assert.NotNull(move);
            Assert.Equal("Thunderbolt", move!.Name);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_HyperBeam_IsNormalSpecial()
        {
            DumpTeam(_team, "Player Team");
            var move = _team.Active.Moves[0] as MoveState;
            _out.WriteLine($"  HyperBeam Element  : {move?.Element}  (expected: Normal)");
            _out.WriteLine($"  HyperBeam Category : {move?.Category}  (expected: Special)");
            Assert.NotNull(move);
            Assert.Equal(PokemonType.Normal, move!.Element);
            Assert.Equal(MoveCategory.Special, move.Category);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_Thunderbolt_IsElectricSpecial()
        {
            DumpTeam(_team, "Player Team");
            var move = _team.Active.Moves[1] as MoveState;
            _out.WriteLine($"  Thunderbolt Element  : {move?.Element}  (expected: Electric)");
            _out.WriteLine($"  Thunderbolt Category : {move?.Category}  (expected: Special)");
            Assert.NotNull(move);
            Assert.Equal(PokemonType.Electric, move!.Element);
            Assert.Equal(MoveCategory.Special, move.Category);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_HyperBeam_HasCorrectPP()
        {
            DumpTeam(_team, "Player Team");
            var move = _team.Active.Moves[0] as MoveState;
            _out.WriteLine($"  HyperBeam PP    : {move?.PP}  (expected: 5)");
            _out.WriteLine($"  HyperBeam MaxPP : {move?.MaxPP}  (expected: 5)");
            Assert.Equal(5, move!.PP);
            Assert.Equal(5, move.MaxPP);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_Thunderbolt_HasCorrectPP()
        {
            DumpTeam(_team, "Player Team");
            var move = _team.Active.Moves[1] as MoveState;
            _out.WriteLine($"  Thunderbolt PP    : {move?.PP}  (expected: 15)");
            _out.WriteLine($"  Thunderbolt MaxPP : {move?.MaxPP}  (expected: 15)");
            Assert.Equal(15, move!.PP);
            Assert.Equal(15, move.MaxPP);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_HasPositiveHP()
        {
            DumpTeam(_team, "Player Team");
            _out.WriteLine($"  MaxHP : {_team.Active.MaxHP}  (expected: > 0)");
            Assert.True(_team.Active.MaxHP > 0);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_StartsAtFullHP()
        {
            DumpTeam(_team, "Player Team");
            _out.WriteLine($"  CurrentHP : {_team.Active.CurrentHP}");
            _out.WriteLine($"  MaxHP     : {_team.Active.MaxHP}");
            Assert.Equal(_team.Active.MaxHP, _team.Active.CurrentHP);
        }

        [Fact]
        public void PlayerTeam_IsNotDefeated()
        {
            DumpTeam(_team, "Player Team");
            _out.WriteLine($"  IsDefeated : {_team.IsDefeated}  (expected: False)");
            Assert.False(_team.IsDefeated);
        }

        [Fact]
        public void PlayerTeam_HasSixAlivePokemon()
        {
            DumpTeam(_team, "Player Team");
            _out.WriteLine($"  AlivePokemonCount : {_team.GetAlivePokemonCount()}  (expected: 6)");
            Assert.Equal(6, _team.GetAlivePokemonCount());
        }

        [Fact]
        public void PlayerTeam_ActiveIndex_IsZeroAtStart()
        {
            DumpTeam(_team, "Player Team");
            _out.WriteLine($"  ActiveIndex : {_team.ActiveIndex}  (expected: 0)");
            Assert.Equal(0, _team.ActiveIndex);
        }

        [Fact]
        public void PlayerTeam_SwitchTo_SlotOne_Succeeds()
        {
            var team = BattleTestFactory.PlayerTeam();
            DumpTeam(team, "Player Team — before switch");
            bool result = team.SwitchTo(1);
            _out.WriteLine($"  SwitchTo(1) returned : {result}   (expected: True)");
            _out.WriteLine($"  ActiveIndex after    : {team.ActiveIndex}  (expected: 1)");
            _out.WriteLine($"  Active Pokémon now   : {team.Active.Name}");
            Assert.True(result);
            Assert.Equal(1, team.ActiveIndex);
        }

        [Fact]
        public void PlayerTeam_SwitchTo_ActiveSlot_ReturnsFalse()
        {
            var team = BattleTestFactory.PlayerTeam();
            DumpTeam(team, "Player Team — switch to active slot");
            bool result = team.SwitchTo(0);
            _out.WriteLine($"  SwitchTo(0) returned : {result}  (expected: False — already active)");
            Assert.False(result);
        }

        [Fact]
        public void PlayerTeam_GetSwitchableIndices_ExcludesActiveSlot()
        {
            DumpTeam(_team, "Player Team");
            var switchable = _team.GetSwitchableIndices();
            _out.WriteLine($"  ActiveIndex       : {_team.ActiveIndex}");
            _out.WriteLine($"  Switchable slots  : [{string.Join(", ", switchable)}]");
            _out.WriteLine($"  Switchable count  : {switchable.Count}  (expected: 5)");
            Assert.DoesNotContain(0, switchable);
            Assert.Equal(5, switchable.Count);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  TeamTranslator — Enemy team (Blastoise × 6, Tackle + Thunderbolt)
    // ═════════════════════════════════════════════════════════════════════════
    public class EnemyTeamTranslatorTests
    {
        private readonly PokemonTeam _team = BattleTestFactory.EnemyTeam();
        private readonly ITestOutputHelper _out;

        public EnemyTeamTranslatorTests(ITestOutputHelper output)
        {
            _out = output;
        }

        // ── Dump helpers ──────────────────────────────────────────────────────

        private void DumpTeam(PokemonTeam team, string label = "PokemonTeam")
        {
            _out.WriteLine($"══ {label} ════════════════════════════════════");
            _out.WriteLine($"  IsDefeated        : {team.IsDefeated}");
            _out.WriteLine($"  ActiveIndex       : {team.ActiveIndex}");
            _out.WriteLine($"  AlivePokemonCount : {team.GetAlivePokemonCount()}");
            _out.WriteLine($"  TotalPokemon      : {team.getAllPokemonCount()}");
            _out.WriteLine($"  SwitchableSlots   : [{string.Join(", ", team.GetSwitchableIndices())}]");
            _out.WriteLine($"  Active → {team.Active.Name}  HP:{team.Active.CurrentHP}/{team.Active.MaxHP}  Fainted:{team.Active.IsFainted}");
            DumpMoves(team.Active);
            _out.WriteLine("══════════════════════════════════════════════");
        }

        private void DumpMoves(Model.Model.Helper.PokemonHelper.PokemonState p)
        {
            _out.WriteLine($"  Moves ({p.Moves.Count}):");
            for (int i = 0; i < p.Moves.Count; i++)
            {
                var m = p.Moves[i] as MoveState;
                if (m != null)
                {
                    _out.WriteLine($"    [{i}] Name     : {m.Name}");
                    _out.WriteLine($"        Type     : {m.Element}");
                    _out.WriteLine($"        Category : {m.Category}");
                    _out.WriteLine($"        PP       : {m.PP}/{m.MaxPP}");
                    _out.WriteLine($"        Priority : {m.Priority}");
                    _out.WriteLine($"        Target   : {m.Target}");
                }
                else
                {
                    _out.WriteLine($"    [{i}] (non-MoveState: {p.Moves[i].GetType().Name})");
                }
            }
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public void EnemyTeam_IsNotNull()
        {
            DumpTeam(_team, "Enemy Team");
            Assert.NotNull(_team);
        }

        [Fact]
        public void EnemyTeam_ActivePokemonIsBlastoise()
        {
            DumpTeam(_team, "Enemy Team");
            Assert.Equal("Blastoise", _team.Active.Name);
        }

        [Fact]
        public void EnemyTeam_ActivePokemonHasTwoMoves()
        {
            DumpTeam(_team, "Enemy Team");
            Assert.Equal(2, _team.Active.Moves.Count);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_FirstMoveIsTackle()
        {
            DumpTeam(_team, "Enemy Team");
            var move = _team.Active.Moves[0] as MoveState;
            _out.WriteLine($"  Moves[0] cast to MoveState: {move?.Name ?? "null"}");
            Assert.NotNull(move);
            Assert.Equal("Tackle", move!.Name);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_SecondMoveIsThunderbolt()
        {
            DumpTeam(_team, "Enemy Team");
            var move = _team.Active.Moves[1] as MoveState;
            _out.WriteLine($"  Moves[1] cast to MoveState: {move?.Name ?? "null"}");
            Assert.NotNull(move);
            Assert.Equal("Thunderbolt", move!.Name);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_Tackle_IsNormalPhysical()
        {
            DumpTeam(_team, "Enemy Team");
            var move = _team.Active.Moves[0] as MoveState;
            _out.WriteLine($"  Tackle Element  : {move?.Element}  (expected: Normal)");
            _out.WriteLine($"  Tackle Category : {move?.Category}  (expected: Physical)");
            Assert.NotNull(move);
            Assert.Equal(PokemonType.Normal, move!.Element);
            Assert.Equal(MoveCategory.Physical, move.Category);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_Thunderbolt_IsElectricSpecial()
        {
            DumpTeam(_team, "Enemy Team");
            var move = _team.Active.Moves[1] as MoveState;
            _out.WriteLine($"  Thunderbolt Element  : {move?.Element}  (expected: Electric)");
            _out.WriteLine($"  Thunderbolt Category : {move?.Category}  (expected: Special)");
            Assert.NotNull(move);
            Assert.Equal(PokemonType.Electric, move!.Element);
            Assert.Equal(MoveCategory.Special, move.Category);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_Tackle_HasCorrectPP()
        {
            DumpTeam(_team, "Enemy Team");
            var move = _team.Active.Moves[0] as MoveState;
            _out.WriteLine($"  Tackle PP : {move?.PP}  (expected: 35)");
            Assert.Equal(35, move!.PP);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_HasPositiveHP()
        {
            DumpTeam(_team, "Enemy Team");
            _out.WriteLine($"  MaxHP : {_team.Active.MaxHP}  (expected: > 0)");
            Assert.True(_team.Active.MaxHP > 0);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_StartsAtFullHP()
        {
            DumpTeam(_team, "Enemy Team");
            _out.WriteLine($"  CurrentHP : {_team.Active.CurrentHP}");
            _out.WriteLine($"  MaxHP     : {_team.Active.MaxHP}");
            Assert.Equal(_team.Active.MaxHP, _team.Active.CurrentHP);
        }

        [Fact]
        public void EnemyTeam_IsNotDefeated()
        {
            DumpTeam(_team, "Enemy Team");
            _out.WriteLine($"  IsDefeated : {_team.IsDefeated}  (expected: False)");
            Assert.False(_team.IsDefeated);
        }

        [Fact]
        public void EnemyTeam_HasSixAlivePokemon()
        {
            DumpTeam(_team, "Enemy Team");
            _out.WriteLine($"  AlivePokemonCount : {_team.GetAlivePokemonCount()}  (expected: 6)");
            Assert.Equal(6, _team.GetAlivePokemonCount());
        }
    }
}