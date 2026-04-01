using PokemonGame.Model.Domain.Pokemon;
using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Helper.MoveHelper;

namespace PokemonGame.Tests
{
    // ═════════════════════════════════════════════════════════════════════════
    //  TeamTranslator — Player team (Charizard × 6, HyperBeam + Thunderbolt)
    // ═════════════════════════════════════════════════════════════════════════
    public class PlayerTeamTranslatorTests
    {
        private readonly PokemonTeam _team = BattleTestFactory.PlayerTeam();

        [Fact]
        public void PlayerTeam_IsNotNull()
            => Assert.NotNull(_team);

        [Fact]
        public void PlayerTeam_ActivePokemonIsCharizard()
            => Assert.Equal("Charizard", _team.Active.Name);

        [Fact]
        public void PlayerTeam_ActivePokemonHasTwoMoves()
            => Assert.Equal(2, _team.Active.Moves.Count);

        [Fact]
        public void PlayerTeam_ActivePokemon_FirstMoveIsHyperBeam()
        {
            var move = _team.Active.Moves[0] as MoveState;
            Assert.NotNull(move);
            Assert.Equal("HyperBeam", move!.Name);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_SecondMoveIsThunderbolt()
        {
            var move = _team.Active.Moves[1] as MoveState;
            Assert.NotNull(move);
            Assert.Equal("Thunderbolt", move!.Name);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_HyperBeam_IsNormalSpecial()
        {
            var move = _team.Active.Moves[0] as MoveState;
            Assert.NotNull(move);
            Assert.Equal(PokemonType.Normal, move!.Element);
            Assert.Equal(MoveCategory.Special, move.Category);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_Thunderbolt_IsElectricSpecial()
        {
            var move = _team.Active.Moves[1] as MoveState;
            Assert.NotNull(move);
            Assert.Equal(PokemonType.Electric, move!.Element);
            Assert.Equal(MoveCategory.Special, move.Category);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_HyperBeam_HasCorrectPP()
        {
            var move = _team.Active.Moves[0] as MoveState;
            Assert.Equal(5, move!.PP);
            Assert.Equal(5, move.MaxPP);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_Thunderbolt_HasCorrectPP()
        {
            var move = _team.Active.Moves[1] as MoveState;
            Assert.Equal(15, move!.PP);
            Assert.Equal(15, move.MaxPP);
        }

        [Fact]
        public void PlayerTeam_ActivePokemon_HasPositiveHP()
            => Assert.True(_team.Active.MaxHP > 0);

        [Fact]
        public void PlayerTeam_ActivePokemon_StartsAtFullHP()
            => Assert.Equal(_team.Active.MaxHP, _team.Active.CurrentHP);

        [Fact]
        public void PlayerTeam_IsNotDefeated()
            => Assert.False(_team.IsDefeated);

        [Fact]
        public void PlayerTeam_HasSixAlivePokemon()
            => Assert.Equal(6, _team.GetAlivePokemonCount());

        [Fact]
        public void PlayerTeam_ActiveIndex_IsZeroAtStart()
            => Assert.Equal(0, _team.ActiveIndex);

        [Fact]
        public void PlayerTeam_SwitchTo_SlotOne_Succeeds()
        {
            var team = BattleTestFactory.PlayerTeam(); // fresh team per test
            Assert.True(team.SwitchTo(1));
            Assert.Equal(1, team.ActiveIndex);
        }

        [Fact]
        public void PlayerTeam_SwitchTo_ActiveSlot_ReturnsFalse()
        {
            var team = BattleTestFactory.PlayerTeam();
            Assert.False(team.SwitchTo(0)); // already slot 0
        }

        [Fact]
        public void PlayerTeam_GetSwitchableIndices_ExcludesActiveSlot()
        {
            var switchable = _team.GetSwitchableIndices();
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

        [Fact]
        public void EnemyTeam_IsNotNull()
            => Assert.NotNull(_team);

        [Fact]
        public void EnemyTeam_ActivePokemonIsBlastoise()
            => Assert.Equal("Blastoise", _team.Active.Name);

        [Fact]
        public void EnemyTeam_ActivePokemonHasTwoMoves()
            => Assert.Equal(2, _team.Active.Moves.Count);

        [Fact]
        public void EnemyTeam_ActivePokemon_FirstMoveIsTackle()
        {
            var move = _team.Active.Moves[0] as MoveState;
            Assert.NotNull(move);
            Assert.Equal("Tackle", move!.Name);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_SecondMoveIsThunderbolt()
        {
            var move = _team.Active.Moves[1] as MoveState;
            Assert.NotNull(move);
            Assert.Equal("Thunderbolt", move!.Name);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_Tackle_IsNormalPhysical()
        {
            var move = _team.Active.Moves[0] as MoveState;
            Assert.NotNull(move);
            Assert.Equal(PokemonType.Normal, move!.Element);
            Assert.Equal(MoveCategory.Physical, move.Category);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_Thunderbolt_IsElectricSpecial()
        {
            var move = _team.Active.Moves[1] as MoveState;
            Assert.NotNull(move);
            Assert.Equal(PokemonType.Electric, move!.Element);
            Assert.Equal(MoveCategory.Special, move.Category);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_Tackle_HasCorrectPP()
        {
            var move = _team.Active.Moves[0] as MoveState;
            Assert.Equal(35, move!.PP);
        }

        [Fact]
        public void EnemyTeam_ActivePokemon_HasPositiveHP()
            => Assert.True(_team.Active.MaxHP > 0);

        [Fact]
        public void EnemyTeam_ActivePokemon_StartsAtFullHP()
            => Assert.Equal(_team.Active.MaxHP, _team.Active.CurrentHP);

        [Fact]
        public void EnemyTeam_IsNotDefeated()
            => Assert.False(_team.IsDefeated);

        [Fact]
        public void EnemyTeam_HasSixAlivePokemon()
            => Assert.Equal(6, _team.GetAlivePokemonCount());
    }
}
