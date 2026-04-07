using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.DesignPatterns;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.ViewModels.Translators;
using Xunit.Abstractions;

namespace PokemonGame.Tests
{
    // ═════════════════════════════════════════════════════════════════════════
    //  MoveTranslator — Hyper Beam tree shape
    // ═════════════════════════════════════════════════════════════════════════
    public class HyperBeamTranslatorTests
    {
        private readonly FakeBattleMoveService _fake = new();
        private readonly MoveTranslator _translator = BattleTestFactory.MoveTranslator();
        private readonly ITestOutputHelper _out;

        public HyperBeamTranslatorTests(ITestOutputHelper output)
        {
            _out = output;
        }

        private MoveTree Tree => _fake.GetMove("HyperBeam")!;

        // ── Shared dump helper ────────────────────────────────────────────────
        private void DumpTree(MoveTree tree)
        {
            _out.WriteLine("══ MoveTree ══════════════════════════════════");
            _out.WriteLine($"  Move.Name     : {tree.Move.Name}");
            _out.WriteLine($"  Move.Element  : {tree.Move.Element}");
            _out.WriteLine($"  Move.Category : {tree.Move.Category}");
            _out.WriteLine($"  Move.PP       : {tree.Move.PP}");
            _out.WriteLine($"  Move.Target   : {tree.Move.Target}");
            _out.WriteLine($"  Priority      : {tree.Priority}");
            _out.WriteLine($"  CritStage     : {tree.CritStage}");
            _out.WriteLine($"  Description   : {tree.Description}");
            _out.WriteLine($"  Attempts.Count: {tree.Attempts.Count}");

            for (int i = 0; i < tree.Attempts.Count; i++)
            {
                DumpAttempt(tree.Attempts[i], $"Attempts[{i}]", indent: 2);
            }
            _out.WriteLine("══════════════════════════════════════════════");
        }

        private void DumpAttempt(MoveAttempt a, string label, int indent)
        {
            string pad = new string(' ', indent);
            _out.WriteLine($"{pad}── {label}");
            _out.WriteLine($"{pad}   Type          : {a.Type}");
            _out.WriteLine($"{pad}   AccuracyValue : {a.AccuracyValue}");

            if (a.ChargeEffect != null)
            {
                DumpEffect(a.ChargeEffect, "ChargeEffect", indent + 3);
            }

            if (a.ReleaseAttempt != null)
            {
                DumpAttempt(a.ReleaseAttempt, "ReleaseAttempt", indent + 3);
            }

            if (a.OnHit != null)
            {
                DumpEffect(a.OnHit, "OnHit", indent + 3);
            }

            if (a.OnMiss != null)
            {
                DumpEffect(a.OnMiss, "OnMiss", indent + 3);
            }

            if (a.After != null)
            {
                DumpEffect(a.After, "After", indent + 3);
            }

            if (a.CascadeSteps.Count > 0)
            {
                for (int i = 0; i < a.CascadeSteps.Count; i++)
                {
                    DumpAttempt(a.CascadeSteps[i], $"CascadeSteps[{i}]", indent + 3);
                }
            }
        }

        private void DumpEffect(MoveEffect e, string label, int indent)
        {
            string pad = new string(' ', indent);
            _out.WriteLine($"{pad}── {label}");
            _out.WriteLine($"{pad}   Type   : {e.Type}");
            _out.WriteLine($"{pad}   Target : {e.Target}");

            if (e.Number != null)
            {
                _out.WriteLine($"{pad}   Number : Type={e.Number.Type}, ExactValue={e.Number.ExactValue}, Min={e.Number.RangeMin}, Max={e.Number.RangeMax}");
            }

            if (e.ChanceProbability.HasValue)
            {
                _out.WriteLine($"{pad}   ChanceProbability : {e.ChanceProbability}");
            }

            if (e.ChanceChild != null)
            {
                DumpEffect(e.ChanceChild, "ChanceChild", indent + 3);
            }

            if (e.SequenceSteps.Count > 0)
            {
                for (int i = 0; i < e.SequenceSteps.Count; i++)
                {
                    DumpEffect(e.SequenceSteps[i], $"SequenceSteps[{i}]", indent + 3);
                }
            }
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public void GetMove_HyperBeam_ReturnsTree()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree);
        }

        [Fact]
        public void GetMove_HyperBeam_MetadataIsCorrect()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("HyperBeam", tree.Move.Name);
            Assert.Equal("Normal", tree.Move.Element);
            Assert.Equal("Special", tree.Move.Category);
            Assert.Equal(5, tree.Move.PP);
            Assert.Equal(0, tree.Priority);
        }

        [Fact]
        public void GetMove_HyperBeam_HasExactlyOneAttempt()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Single(tree.Attempts);
        }

        [Fact]
        public void GetMove_HyperBeam_RootAttemptIsCharge()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Charge", tree.Attempts[0].Type);
        }

        [Fact]
        public void GetMove_HyperBeam_ChargeEffectIsNoEffect()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("NoEffect", tree.Attempts[0].ChargeEffect!.Type);
        }

        [Fact]
        public void GetMove_HyperBeam_ReleaseAttemptExists()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree.Attempts[0].ReleaseAttempt);
        }

        [Fact]
        public void GetMove_HyperBeam_ReleaseAttemptIsAlwaysHit()
        {
            var tree = Tree;
            DumpTree(tree);
            var release = tree.Attempts[0].ReleaseAttempt!;
            _out.WriteLine($"  ReleaseAttempt.Type          : {release.Type}");
            _out.WriteLine($"  ReleaseAttempt.AccuracyValue : {release.AccuracyValue}");
            Assert.Equal("Attempt", release.Type);
            Assert.Equal(1.0, release.AccuracyValue);
        }

        [Fact]
        public void GetMove_HyperBeam_ReleaseOnHitIsFormulaDamage150()
        {
            var tree = Tree;
            DumpTree(tree);
            var onHit = tree.Attempts[0].ReleaseAttempt!.OnHit!;
            _out.WriteLine($"  ReleaseAttempt.OnHit.Type              : {onHit.Type}");
            _out.WriteLine($"  ReleaseAttempt.OnHit.Number.Type       : {onHit.Number?.Type}");
            _out.WriteLine($"  ReleaseAttempt.OnHit.Number.ExactValue : {onHit.Number?.ExactValue}");
            Assert.Equal("FormulaDamage", onHit.Type);
            Assert.Equal(150.0, onHit.Number!.ExactValue);
        }

        [Fact]
        public void Translate_HyperBeam_MetadataIsCorrect()
        {
            var domain = _translator.Translate("HyperBeam");
            _out.WriteLine("══ MoveDomain ════════════════════════════════");
            _out.WriteLine($"  Name     : {domain.Name}");
            _out.WriteLine($"  Element  : {domain.Element}");
            _out.WriteLine($"  Category : {domain.Category}");
            _out.WriteLine($"  PP       : {domain.PP}");
            _out.WriteLine($"  MaxPP    : {domain.MaxPP}");
            _out.WriteLine($"  Target   : {domain.Target}");
            _out.WriteLine($"  Priority : {domain.Priority}");
            _out.WriteLine("══════════════════════════════════════════════");
            Assert.Equal("HyperBeam", domain.Name);
            Assert.Equal(PokemonType.Normal, domain.Element);
            Assert.Equal(MoveCategory.Special, domain.Category);
            Assert.Equal(5, domain.PP);
            Assert.Equal(5, domain.MaxPP);
        }

        [Fact]
        public void TranslateAttempt_HyperBeam_ReturnsChargeInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var attempt = _translator.TranslateAttempt(tree.Attempts[0]);
            _out.WriteLine($"  TranslateAttempt result type : {attempt.GetType().Name}");
            Assert.IsType<Charge>(attempt);
        }

        [Fact]
        public void TranslateEffect_HyperBeam_ChargeEffectIsNoEffectInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Attempts[0].ChargeEffect!);
            _out.WriteLine($"  TranslateEffect (ChargeEffect) result type : {effect.GetType().Name}");
            Assert.IsType<NoEffect>(effect);
        }

        [Fact]
        public void TranslateEffect_HyperBeam_ReleaseOnHitIsFormulaDamageInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Attempts[0].ReleaseAttempt!.OnHit!);
            _out.WriteLine($"  TranslateEffect (ReleaseAttempt.OnHit) result type : {effect.GetType().Name}");
            Assert.IsType<FormulaDamage>(effect);
        }

        [Fact]
        public void TranslateNumber_HyperBeam_ReturnsExactly150()
        {
            var tree = Tree;
            DumpTree(tree);
            var number = _translator.TranslateNumber(tree.Attempts[0].ReleaseAttempt!.OnHit!.Number!);
            _out.WriteLine($"  TranslateNumber result type : {number.GetType().Name}");
            Assert.IsType<Exactly>(number);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  MoveTranslator — Tackle tree shape
    // ═════════════════════════════════════════════════════════════════════════
    public class TackleTranslatorTests
    {
        private readonly FakeBattleMoveService _fake = new();
        private readonly MoveTranslator _translator = BattleTestFactory.MoveTranslator();
        private readonly ITestOutputHelper _out;

        public TackleTranslatorTests(ITestOutputHelper output)
        {
            _out = output;
        }

        private MoveTree Tree => _fake.GetMove("Tackle")!;

        private void DumpTree(MoveTree tree)
        {
            _out.WriteLine("══ MoveTree ══════════════════════════════════");
            _out.WriteLine($"  Move.Name     : {tree.Move.Name}");
            _out.WriteLine($"  Move.Element  : {tree.Move.Element}");
            _out.WriteLine($"  Move.Category : {tree.Move.Category}");
            _out.WriteLine($"  Move.PP       : {tree.Move.PP}");
            _out.WriteLine($"  Attempts.Count: {tree.Attempts.Count}");

            if (tree.Attempts.Count > 0)
            {
                var a = tree.Attempts[0];
                _out.WriteLine($"  Attempts[0].Type          : {a.Type}");
                _out.WriteLine($"  Attempts[0].AccuracyValue : {a.AccuracyValue}");

                if (a.OnHit != null)
                {
                    _out.WriteLine($"  Attempts[0].OnHit.Type             : {a.OnHit.Type}");
                    _out.WriteLine($"  Attempts[0].OnHit.Target           : {a.OnHit.Target}");
                    _out.WriteLine($"  Attempts[0].OnHit.Number?.Type     : {a.OnHit.Number?.Type}");
                    _out.WriteLine($"  Attempts[0].OnHit.Number?.ExactVal : {a.OnHit.Number?.ExactValue}");
                    _out.WriteLine($"  Attempts[0].OnHit.ChanceChild      : {(a.OnHit.ChanceChild == null ? "null" : a.OnHit.ChanceChild.Type)}");
                }
            }
            _out.WriteLine("══════════════════════════════════════════════");
        }

        [Fact]
        public void GetMove_Tackle_ReturnsTree()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree);
        }

        [Fact]
        public void GetMove_Tackle_MetadataIsCorrect()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Tackle", tree.Move.Name);
            Assert.Equal("Normal", tree.Move.Element);
            Assert.Equal("Physical", tree.Move.Category);
            Assert.Equal(35, tree.Move.PP);
        }

        [Fact]
        public void GetMove_Tackle_HasExactlyOneAttempt()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Single(tree.Attempts);
        }

        [Fact]
        public void GetMove_Tackle_AttemptIsAlwaysHit()
        {
            var tree = Tree;
            DumpTree(tree);
            var attempt = tree.Attempts[0];
            Assert.Equal("Attempt", attempt.Type);
            Assert.Equal(1.0, attempt.AccuracyValue);
        }

        [Fact]
        public void GetMove_Tackle_OnHitIsFormulaDamage40()
        {
            var tree = Tree;
            DumpTree(tree);
            var onHit = tree.Attempts[0].OnHit!;
            Assert.Equal("FormulaDamage", onHit.Type);
            Assert.Equal(40.0, onHit.Number!.ExactValue);
        }

        [Fact]
        public void GetMove_Tackle_HasNoSideEffect()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Null(tree.Attempts[0].OnHit!.ChanceChild);
        }

        [Fact]
        public void Translate_Tackle_MetadataIsCorrect()
        {
            var domain = _translator.Translate("Tackle");
            _out.WriteLine("══ MoveDomain ════════════════════════════════");
            _out.WriteLine($"  Name     : {domain.Name}");
            _out.WriteLine($"  Element  : {domain.Element}");
            _out.WriteLine($"  Category : {domain.Category}");
            _out.WriteLine($"  PP       : {domain.PP}");
            _out.WriteLine($"  MaxPP    : {domain.MaxPP}");
            _out.WriteLine($"  Target   : {domain.Target}");
            _out.WriteLine("══════════════════════════════════════════════");
            Assert.Equal("Tackle", domain.Name);
            Assert.Equal(PokemonType.Normal, domain.Element);
            Assert.Equal(MoveCategory.Physical, domain.Category);
            Assert.Equal(35, domain.PP);
        }

        [Fact]
        public void TranslateAttempt_Tackle_ReturnsAttemptInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var attempt = _translator.TranslateAttempt(tree.Attempts[0]);
            _out.WriteLine($"  TranslateAttempt result type : {attempt.GetType().Name}");
            Assert.IsType<Attempt>(attempt);
        }

        [Fact]
        public void TranslateEffect_Tackle_OnHitIsFormulaDamageInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Attempts[0].OnHit!);
            _out.WriteLine($"  TranslateEffect (OnHit) result type : {effect.GetType().Name}");
            Assert.IsType<FormulaDamage>(effect);
        }

        [Fact]
        public void TranslateNumber_Tackle_ReturnsExactlyInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var number = _translator.TranslateNumber(tree.Attempts[0].OnHit!.Number!);
            _out.WriteLine($"  TranslateNumber result type : {number.GetType().Name}");
            Assert.IsType<Exactly>(number);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  MoveTranslator — Thunderbolt tree shape
    // ═════════════════════════════════════════════════════════════════════════
    public class ThunderboltTranslatorTests
    {
        private readonly FakeBattleMoveService _fake = new();
        private readonly MoveTranslator _translator = BattleTestFactory.MoveTranslator();
        private readonly ITestOutputHelper _out;

        public ThunderboltTranslatorTests(ITestOutputHelper output)
        {
            _out = output;
        }

        private MoveTree Tree => _fake.GetMove("Thunderbolt")!;

        private void DumpTree(MoveTree tree)
        {
            _out.WriteLine("══ MoveTree ══════════════════════════════════");
            _out.WriteLine($"  Move.Name     : {tree.Move.Name}");
            _out.WriteLine($"  Move.Element  : {tree.Move.Element}");
            _out.WriteLine($"  Move.Category : {tree.Move.Category}");
            _out.WriteLine($"  Move.PP       : {tree.Move.PP}");
            _out.WriteLine($"  Attempts.Count: {tree.Attempts.Count}");

            if (tree.Attempts.Count > 0)
            {
                var a = tree.Attempts[0];
                _out.WriteLine($"  Attempts[0].Type          : {a.Type}");
                _out.WriteLine($"  Attempts[0].AccuracyValue : {a.AccuracyValue}");

                if (a.OnHit != null)
                {
                    _out.WriteLine($"  Attempts[0].OnHit.Type              : {a.OnHit.Type}");
                    _out.WriteLine($"  Attempts[0].OnHit.SequenceSteps.Count: {a.OnHit.SequenceSteps.Count}");

                    for (int i = 0; i < a.OnHit.SequenceSteps.Count; i++)
                    {
                        var step = a.OnHit.SequenceSteps[i];
                        _out.WriteLine($"    SequenceSteps[{i}].Type              : {step.Type}");
                        _out.WriteLine($"    SequenceSteps[{i}].Target            : {step.Target}");

                        if (step.Number != null)
                        {
                            _out.WriteLine($"    SequenceSteps[{i}].Number.ExactValue: {step.Number.ExactValue}");
                        }

                        if (step.ChanceProbability.HasValue)
                        {
                            _out.WriteLine($"    SequenceSteps[{i}].ChanceProbability: {step.ChanceProbability}");
                        }

                        if (step.ChanceChild != null)
                        {
                            _out.WriteLine($"    SequenceSteps[{i}].ChanceChild.Type : {step.ChanceChild.Type}");
                            _out.WriteLine($"    SequenceSteps[{i}].ChanceChild.Target: {step.ChanceChild.Target}");
                        }
                    }
                }
            }
            _out.WriteLine("══════════════════════════════════════════════");
        }

        [Fact]
        public void GetMove_Thunderbolt_ReturnsTree()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree);
        }

        [Fact]
        public void GetMove_Thunderbolt_MetadataIsCorrect()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Thunderbolt", tree.Move.Name);
            Assert.Equal("Electric", tree.Move.Element);
            Assert.Equal("Special", tree.Move.Category);
            Assert.Equal(15, tree.Move.PP);
        }

        [Fact]
        public void GetMove_Thunderbolt_HasExactlyOneAttempt()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Single(tree.Attempts);
        }

        [Fact]
        public void GetMove_Thunderbolt_AttemptIsAlwaysHit()
        {
            var tree = Tree;
            DumpTree(tree);
            var attempt = tree.Attempts[0];
            Assert.Equal("Attempt", attempt.Type);
            Assert.Equal(1.0, attempt.AccuracyValue);
        }

        [Fact]
        public void GetMove_Thunderbolt_OnHitIsSequence()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Sequence", tree.Attempts[0].OnHit!.Type);
        }

        [Fact]
        public void GetMove_Thunderbolt_SequenceHasTwoSteps()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal(2, tree.Attempts[0].OnHit!.SequenceSteps.Count);
        }

        [Fact]
        public void GetMove_Thunderbolt_FirstStepIsFormulaDamage90()
        {
            var tree = Tree;
            DumpTree(tree);
            var step0 = tree.Attempts[0].OnHit!.SequenceSteps[0];
            Assert.Equal("FormulaDamage", step0.Type);
            Assert.Equal(90.0, step0.Number!.ExactValue);
        }

        [Fact]
        public void GetMove_Thunderbolt_SecondStepIsChanceParalyze()
        {
            var tree = Tree;
            DumpTree(tree);
            var step1 = tree.Attempts[0].OnHit!.SequenceSteps[1];
            Assert.Equal("Chance", step1.Type);
            Assert.Equal(0.10, step1.ChanceProbability!.Value, precision: 3);
            Assert.NotNull(step1.ChanceChild);
            Assert.Equal("Paralyze", step1.ChanceChild!.Type);
            Assert.Equal("Defender", step1.ChanceChild.Target);
        }

        [Fact]
        public void Translate_Thunderbolt_MetadataIsCorrect()
        {
            var domain = _translator.Translate("Thunderbolt");
            _out.WriteLine("══ MoveDomain ════════════════════════════════");
            _out.WriteLine($"  Name     : {domain.Name}");
            _out.WriteLine($"  Element  : {domain.Element}");
            _out.WriteLine($"  Category : {domain.Category}");
            _out.WriteLine($"  PP       : {domain.PP}");
            _out.WriteLine($"  MaxPP    : {domain.MaxPP}");
            _out.WriteLine($"  Target   : {domain.Target}");
            _out.WriteLine("══════════════════════════════════════════════");
            Assert.Equal("Thunderbolt", domain.Name);
            Assert.Equal(PokemonType.Electric, domain.Element);
            Assert.Equal(MoveCategory.Special, domain.Category);
            Assert.Equal(15, domain.PP);
        }

        [Fact]
        public void TranslateAttempt_Thunderbolt_ReturnsAttemptInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var attempt = _translator.TranslateAttempt(tree.Attempts[0]);
            _out.WriteLine($"  TranslateAttempt result type : {attempt.GetType().Name}");
            Assert.IsType<Attempt>(attempt);
        }

        [Fact]
        public void TranslateEffect_Thunderbolt_OnHitIsSequenceInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Attempts[0].OnHit!);
            _out.WriteLine($"  TranslateEffect (OnHit) result type : {effect.GetType().Name}");
            Assert.IsType<Sequence>(effect);
        }

        [Fact]
        public void TranslateEffect_Thunderbolt_FirstStepIsFormulaDamageInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Attempts[0].OnHit!.SequenceSteps[0]);
            _out.WriteLine($"  TranslateEffect (SequenceSteps[0]) result type : {effect.GetType().Name}");
            Assert.IsType<FormulaDamage>(effect);
        }

        [Fact]
        public void TranslateEffect_Thunderbolt_SecondStepIsChanceInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Attempts[0].OnHit!.SequenceSteps[1]);
            _out.WriteLine($"  TranslateEffect (SequenceSteps[1]) result type : {effect.GetType().Name}");
            Assert.IsType<Chance>(effect);
        }
    }
}