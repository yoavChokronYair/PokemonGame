using PokemonGame.Model.Domain.Move;
using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using Xunit.Abstractions;

namespace PokemonGame.Tests
{
    // ═════════════════════════════════════════════════════════════════════════
    //  TeamTranslator — Player team (Charizard × 6, Blaze Ability)
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
            _out.WriteLine($"  Active Pokémon    : {team.Active.Name}");
            _out.WriteLine($"  Ability           : {((AbilityState)(_team.Active.Ability)).Name ?? "NULL"}");
            _out.WriteLine($"  IsDefeated        : {team.IsDefeated}");
            _out.WriteLine($"  ActiveIndex       : {team.ActiveIndex}");
            _out.WriteLine($"  Alive Count       : {team.GetAlivePokemonCount()} / {team.getAllPokemonCount()}");
            _out.WriteLine($"  HP Status         : {team.Active.CurrentHP}/{team.Active.MaxHP}");

            DumpMoves(team.Active);
            _out.WriteLine("══════════════════════════════════════════════");
        }

        private void DumpMoves(PokemonState p)
        {
            _out.WriteLine($"  Moves ({p.Moves.Count}):");
            var moves = p.Moves.OfType<MoveState>().ToList();
            for (int i = 0; i < moves.Count; i++)
            {
                var m = moves[i];
                _out.WriteLine($"    [{i}] {m.Name,-12} | {m.Element,-8} | {m.Category,-8} | PP: {m.PP}/{m.MaxPP}");
            }
        }

        // ── Identity & Ability Tests ──────────────────────────────────────────

        [Fact]
        public void PlayerTeam_IsNotNull()
        {
            Assert.NotNull(_team);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_IdentityAndAbility()
        {
            DumpTeam(_team, "Player Team Identity");
            Assert.Equal("Charizard", _team.Active.Name);
            Assert.NotNull(_team.Active.Ability);
            // Verify ability name (assuming Blaze is default for Charizard)
            Assert.Equal("Blaze", ((AbilityState)(_team.Active.Ability)).Name);
        }



        // ── Move Tests ────────────────────────────────────────────────────────

        [Fact]
        public void PlayerTeam_ActivePokemon_Moves_VerifyDetails()
        {
            var moves = _team.Active.Moves.OfType<MoveState>().ToList();
            Assert.Equal(2, moves.Count);

            // HyperBeam Check
            Assert.Equal("HyperBeam", moves[0].Name);
            Assert.Equal(PokemonType.Normal, moves[0].Element);
            Assert.Equal(5, moves[0].MaxPP);

            // Thunderbolt Check
            Assert.Equal("Thunderbolt", moves[1].Name);
            Assert.Equal(PokemonType.Electric, moves[1].Element);
        }

        // ── Health & Team State ───────────────────────────────────────────────

        [Fact]
        public void PlayerTeam_Health_IsFullOnLoad()
        {
            Assert.True(_team.Active.MaxHP > 0);
            Assert.Equal(_team.Active.MaxHP, _team.Active.CurrentHP);
        }

        [Fact]
        public void PlayerTeam_Switching_LogicWorks()
        {
            var team = BattleTestFactory.PlayerTeam();
            bool canSwitch = team.SwitchTo(1);

            Assert.True(canSwitch);
            Assert.Equal(1, team.ActiveIndex);
            Assert.False(team.SwitchTo(1)); // Cannot switch to already active
        }

        [Fact]
        public void PlayerTeam_GetSwitchableIndices_CorrectCount()
        {
            var switchable = _team.GetSwitchableIndices();
            Assert.Equal(5, switchable.Count);
            Assert.DoesNotContain(_team.ActiveIndex, switchable);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  TeamTranslator — Enemy team (Blastoise × 6, Torrent Ability)
    // ═════════════════════════════════════════════════════════════════════════
    public class EnemyTeamTranslatorTests
    {
        private readonly PokemonTeam _team = BattleTestFactory.EnemyTeam();
        private readonly ITestOutputHelper _out;

        public EnemyTeamTranslatorTests(ITestOutputHelper output)
        {
            _out = output;
        }

        private void DumpTeam(PokemonTeam team, string label = "PokemonTeam")
        {
            _out.WriteLine($"══ {label} ════════════════════════════════════");
            _out.WriteLine($"  Active Pokémon    : {team.Active.Name}");
            _out.WriteLine($"  Ability           : {((AbilityState)(_team.Active.Ability)).Name ?? "NULL"}");
            _out.WriteLine($"  Alive Count       : {team.GetAlivePokemonCount()}");
            _out.WriteLine("══════════════════════════════════════════════");
        }

        [Fact]
        public void EnemyTeam_Identity_VerifyBlastoiseAndTorrent()
        {
            DumpTeam(_team, "Enemy Team Identity");
            Assert.Equal("Blastoise", _team.Active.Name);
            Assert.NotNull(_team.Active.Ability);
            Assert.Equal("Torrent", ((AbilityState)(_team.Active.Ability)).Name);
        }

        [Fact]
        public void EnemyTeam_Moves_VerifyTackle()
        {
            var moves = _team.Active.Moves.OfType<MoveState>().ToList();
            var tackle = moves.FirstOrDefault(m => m.Name == "Tackle");

            Assert.NotNull(tackle);
            Assert.Equal(PokemonType.Normal, tackle.Element);
            Assert.Equal(MoveCategory.Physical, tackle.Category);
            Assert.Equal(35, tackle.MaxPP);
        }

        [Fact]
        public void EnemyTeam_State_IsNotDefeated()
        {
            Assert.False(_team.IsDefeated);
            Assert.Equal(6, _team.GetAlivePokemonCount());
        }
    }
}