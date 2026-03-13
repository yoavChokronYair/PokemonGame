using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Helper.DesignPatterns;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Translators;

namespace PokemonGame.Tests
{
    // ── Fake service — no DB, no SQLite ─────────────────────────────────────
    internal class FakeMoveService : IMoveService
    {
        public MoveTree? GetMove(string name)
        {
            if (name != "Flamethrower") return null;

            var move = new MoveData
            {
                Id = 1,
                Name = "Flamethrower",
                Element = "Fire",
                Category = "Special",
                Target = "Opponent",
                PP = 15,
                Priority = 0,
                CritStage = 0,
                Description = "The target is scorched with an intense blast of fire.",
            };

            // Sequence { FormulaDamage(90), Chance(0.10, Burn) }
            var formulaDamageEffect = new MoveEffect
            {
                Id = 10,
                Type = "FormulaDamage",
                Target = "Defender",
                Number = new MoveNumber { Id = 1, Type = "Exactly", ExactValue = 90.0 },
            };

            var burnEffect = new MoveEffect
            {
                Id = 12,
                Type = "Burn",
                Target = "Defender",
            };

            var chanceEffect = new MoveEffect
            {
                Id = 11,
                Type = "Chance",
                ChanceProbability = 0.10,
                ChanceChild = burnEffect,
            };

            var sequenceEffect = new MoveEffect
            {
                Id = 9,
                Type = "Sequence",
                SequenceSteps = new List<MoveEffect> { formulaDamageEffect, chanceEffect },
            };

            var attempt = new MoveAttempt
            {
                Id = 1,
                Type = "Attempt",
                AccuracyValue = 1.0,
                OnHit = sequenceEffect,
            };

            return new MoveTree
            {
                Move = move,
                Priority = 0,
                CritStage = 0,
                Description = move.Description,
                Attempts = new List<MoveAttempt> { attempt },
            };
        }
    }

    // ── Tests ────────────────────────────────────────────────────────────────
    public class MoveTranslatorTests
    {
        private readonly FakeMoveService _fake = new();
        private readonly MoveTranslator _translator;

        // Helper: grab the Flamethrower tree without hitting a DB
        private MoveTree Tree => _fake.GetMove("Flamethrower")!;

        public MoveTranslatorTests()
        {
            _translator = new MoveTranslator(_fake);
        }

        // ── FakeMoveService: tree shape ──────────────────────────────────────

        [Fact]
        public void GetMove_Flamethrower_ReturnsTree()
            => Assert.NotNull(Tree);

        [Fact]
        public void GetMove_Flamethrower_MoveMetadataIsCorrect()
        {
            var tree = Tree;
            Assert.Equal("Flamethrower", tree.Move.Name);
            Assert.Equal("Fire", tree.Move.Element);
            Assert.Equal("Special", tree.Move.Category);
            Assert.Equal("Opponent", tree.Move.Target);
            Assert.Equal(15, tree.Move.PP);
            Assert.Equal(0, tree.Priority);
            Assert.Equal(0, tree.CritStage);
            Assert.False(string.IsNullOrWhiteSpace(tree.Description));
        }

        [Fact]
        public void GetMove_Flamethrower_HasExactlyOneAttempt()
            => Assert.Single(Tree.Attempts);

        [Fact]
        public void GetMove_Flamethrower_AttemptIsAlwaysHit()
        {
            var attempt = Tree.Attempts[0];
            Assert.Equal("Attempt", attempt.Type);
            Assert.Equal(1.0, attempt.AccuracyValue);
        }

        [Fact]
        public void GetMove_Flamethrower_OnHitIsSequence()
        {
            var attempt = Tree.Attempts[0];
            Assert.NotNull(attempt.OnHit);
            Assert.Equal("Sequence", attempt.OnHit!.Type);
        }

        [Fact]
        public void GetMove_Flamethrower_SequenceHasTwoSteps()
            => Assert.Equal(2, Tree.Attempts[0].OnHit!.SequenceSteps.Count);

        [Fact]
        public void GetMove_Flamethrower_FirstStepIsFormulaDamage()
        {
            var step0 = Tree.Attempts[0].OnHit!.SequenceSteps[0];
            Assert.Equal("FormulaDamage", step0.Type);
            Assert.Equal("Defender", step0.Target);
            Assert.NotNull(step0.Number);
            Assert.Equal("Exactly", step0.Number!.Type);
            Assert.Equal(90.0, step0.Number.ExactValue);
        }

        [Fact]
        public void GetMove_Flamethrower_SecondStepIsChanceBurn()
        {
            var step1 = Tree.Attempts[0].OnHit!.SequenceSteps[1];
            Assert.Equal("Chance", step1.Type);
            Assert.Equal(0.10, step1.ChanceProbability!.Value, precision: 3);
            Assert.NotNull(step1.ChanceChild);
            Assert.Equal("Burn", step1.ChanceChild!.Type);
            Assert.Equal("Defender", step1.ChanceChild.Target);
        }

        [Fact]
        public void GetMove_UnknownMove_ReturnsNull()
            => Assert.Null(_fake.GetMove("NotARealMove"));

        // ── MoveTranslator: MoveDomain shape ─────────────────────────────────

        [Fact]
        public void Translate_Flamethrower_ReturnsDomain()
            => Assert.NotNull(_translator.Translate("Flamethrower"));

        [Fact]
        public void Translate_Flamethrower_MetadataIsCorrect()
        {
            var domain = _translator.Translate("Flamethrower");
            Assert.Equal("Flamethrower", domain.Name);
            Assert.Equal(PokemonType.Fire, domain.Element);
            Assert.Equal(MoveCategory.Special, domain.Category);
            Assert.Equal(MoveTarget.Opponent, domain.Target);
            Assert.Equal(15, domain.PP);
            Assert.Equal(15, domain.MaxPP);
            Assert.Equal(0, domain.Priority);
            Assert.Equal(0, domain.CritStage);
            Assert.False(string.IsNullOrWhiteSpace(domain.Description));
        }

        [Fact]
        public void Translate_UnknownMove_ThrowsInvalidOperation()
            => Assert.Throws<InvalidOperationException>(() =>
                _translator.Translate("NotARealMove"));

        // ── MoveTranslator: attempt tree wiring ──────────────────────────────

        [Fact]
        public void TranslateAttempt_Flamethrower_ReturnsAttemptInstance()
            => Assert.IsType<Attempt>(_translator.TranslateAttempt(Tree.Attempts[0]));

        [Fact]
        public void TranslateEffect_Sequence_ReturnsSequenceInstance()
            => Assert.IsType<Sequence>(_translator.TranslateEffect(Tree.Attempts[0].OnHit!));

        [Fact]
        public void TranslateEffect_FormulaDamage_ReturnsFormulaDamageInstance()
            => Assert.IsType<FormulaDamage>(_translator.TranslateEffect(Tree.Attempts[0].OnHit!.SequenceSteps[0]));

        [Fact]
        public void TranslateEffect_Chance_ReturnsChanceInstance()
            => Assert.IsType<Chance>(_translator.TranslateEffect(Tree.Attempts[0].OnHit!.SequenceSteps[1]));

        [Fact]
        public void TranslateNumber_Exactly_ReturnsExactlyInstance()
            => Assert.IsType<Exactly>(_translator.TranslateNumber(Tree.Attempts[0].OnHit!.SequenceSteps[0].Number!));

        // ── Full pipeline ────────────────────────────────────────────────────

        [Fact]
        public void FullPipeline_Flamethrower_DomainAndAttemptAreConsistent()
        {
            var tree = Tree;
            var domain = _translator.Translate("Flamethrower");
            var attempt = _translator.TranslateAttempt(tree.Attempts[0]);

            Assert.Equal(tree.Move.Name, domain.Name);
            Assert.Equal(tree.Move.PP, domain.PP);
            Assert.NotNull(attempt);
            Assert.IsType<Attempt>(attempt);
        }
    }
}