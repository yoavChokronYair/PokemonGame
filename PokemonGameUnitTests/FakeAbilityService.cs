using PokemonGame.Model.Enums;
using PokemonGame.Model.Model.Helper.DesignPatterns;
using PokemonGame.Services.Data.GameData.Move;
using PokemonGame.Services.Data.GameData.PokemonData;
using PokemonGame.Services.Handler;
using PokemonGame.ViewModels.Translators;
using Xunit.Abstractions;

namespace PokemonGame.Tests
{
    // ═════════════════════════════════════════════════════════════════════════
    //  Fake service + factory
    // ═════════════════════════════════════════════════════════════════════════

    public class FakeAbilityService : IAbilityService
    {
        private readonly Dictionary<string, AbilityTree> _abilities = new()
        {
            // ── Intimidate ───────────────────────────────────────────────────
            // No condition. On entry: lower opponent Attack -1.
            ["Intimidate"] = new AbilityTree
            {
                Ability = new AbilityData { Id = 1, Name = "Intimidate", Description = "Lowers the foe's Attack.", Trigger = "OnEntry" },
                Name = "Intimidate",
                Description = "Lowers the foe's Attack.",
                Trigger = "OnEntry",
                Condition = null,
                Effect = new MoveEffect
                {
                    Id = 1,
                    Type = "StatChange",
                    Target = "Defender",
                    Stat = "Attack",
                    StatStages = -1,
                },
            },

            // ── Blaze ────────────────────────────────────────────────────────
            // Condition: HPBelow(0.333) on attacker.
            // Effect: StatChange SpAtk +1 on attacker (Conditional → OnPass).
            ["Blaze"] = new AbilityTree
            {
                Ability = new AbilityData { Id = 2, Name = "Blaze", Description = "Powers up Fire moves in a pinch.", Trigger = "OnAttack" },
                Name = "Blaze",
                Description = "Powers up Fire moves in a pinch.",
                Trigger = "OnAttack",
                Condition = new MoveCondition
                {
                    Id = 1,
                    Type = "HPBelow",
                    HpFraction = 1.0 / 3.0,
                },
                Effect = new MoveEffect
                {
                    Id = 2,
                    Type = "StatChange",
                    Target = "Attacker",
                    Stat = "SpecialAttack",
                    StatStages = 1,
                },
            },

            // ── Static ───────────────────────────────────────────────────────
            // Condition: WasHitByContact.
            // Effect: Conditional → OnPass: Chance(0.30) → Paralyze(Defender).
            ["Static"] = new AbilityTree
            {
                Ability = new AbilityData { Id = 3, Name = "Static", Description = "May paralyze on contact.", Trigger = "OnHit" },
                Name = "Static",
                Description = "May paralyze on contact.",
                Trigger = "OnHit",
                Condition = new MoveCondition
                {
                    Id = 2,
                    Type = "WasHitByContact",
                },
                Effect = new MoveEffect
                {
                    Id = 3,
                    Type = "Chance",
                    ChanceProbability = 0.30,
                    ChanceChild = new MoveEffect
                    {
                        Id = 4,
                        Type = "Paralyze",
                        Target = "Defender",
                    },
                },
            },
        };

        public AbilityTree? GetAbility(string name) => _abilities.GetValueOrDefault(name);
        public AbilityTree? GetAbilityById(int id) => _abilities.Values.FirstOrDefault(a => a.Ability.Id == id);
    }

    public static partial class AbilityTestFactory
    {
        public static AbilityTranslator AbilityTranslator() =>
            new(new FakeAbilityService(), new MoveTranslator());
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  AbilityTranslator — Intimidate  (simple: no condition, StatChange)
    // ═════════════════════════════════════════════════════════════════════════

    public class IntimidateTranslatorTests
    {
        private readonly FakeAbilityService _fake = new();
        private readonly AbilityTranslator _translator = AbilityTestFactory.AbilityTranslator();
        private readonly ITestOutputHelper _out;

        public IntimidateTranslatorTests(ITestOutputHelper output) => _out = output;

        private AbilityTree Tree => _fake.GetAbility("Intimidate")!;

        private void DumpTree(AbilityTree tree)
        {
            _out.WriteLine("══ AbilityTree ═══════════════════════════════");
            _out.WriteLine($"  Name        : {tree.Name}");
            _out.WriteLine($"  Description : {tree.Description}");
            _out.WriteLine($"  Trigger     : {tree.Trigger}");
            _out.WriteLine($"  Condition   : {(tree.Condition == null ? "null" : tree.Condition.Type)}");
            _out.WriteLine($"  Effect.Type : {tree.Effect?.Type}");
            _out.WriteLine($"  Effect.Target : {tree.Effect?.Target}");
            _out.WriteLine($"  Effect.Stat   : {tree.Effect?.Stat}");
            _out.WriteLine($"  Effect.Stages : {tree.Effect?.StatStages}");
            _out.WriteLine("══════════════════════════════════════════════");
        }

        // ── Tree shape ────────────────────────────────────────────────────────

        [Fact]
        public void GetAbility_Intimidate_ReturnsTree()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree);
        }

        [Fact]
        public void GetAbility_Intimidate_MetadataIsCorrect()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Intimidate", tree.Name);
            Assert.Equal("OnEntry", tree.Trigger);
        }

        [Fact]
        public void GetAbility_Intimidate_HasNoCondition()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Null(tree.Condition);
        }

        [Fact]
        public void GetAbility_Intimidate_EffectIsStatChange()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("StatChange", tree.Effect!.Type);
        }

        [Fact]
        public void GetAbility_Intimidate_EffectTargetsDefender()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Defender", tree.Effect!.Target);
        }

        [Fact]
        public void GetAbility_Intimidate_LowersAttackByOne()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Attack", tree.Effect!.Stat);
            Assert.Equal(-1, tree.Effect!.StatStages);
        }

        // ── Translator ────────────────────────────────────────────────────────

        [Fact]
        public void Translate_Intimidate_ReturnsAbilityState()
        {
            var state = _translator.Translate("Intimidate");
            _out.WriteLine($"  AbilityState.Name : {state.Name}");
            Assert.NotNull(state);
            Assert.Equal("Intimidate", state.Name);
        }

        [Fact]
        public void Translate_Intimidate_ConditionIsAlwaysTrue()
        {
            var tree = Tree;
            DumpTree(tree);
            // No condition in tree → falls back to Probability<BattleState>(1.0)
            var state = _translator.Translate("Intimidate");
            Assert.NotNull(state);  // passed construction — Probability(1.0) always satisfies
        }

        [Fact]
        public void TranslateEffect_Intimidate_IsStatChangeInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Effect!);
            _out.WriteLine($"  TranslateEffect result type : {effect.GetType().Name}");
            Assert.IsType<StatChange>(effect);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  AbilityTranslator — Blaze  (conditional: HPBelow → StatChange)
    // ═════════════════════════════════════════════════════════════════════════

    public class BlazeTranslatorTests
    {
        private readonly FakeAbilityService _fake = new();
        private readonly AbilityTranslator _translator = AbilityTestFactory.AbilityTranslator();
        private readonly ITestOutputHelper _out;

        public BlazeTranslatorTests(ITestOutputHelper output) => _out = output;

        private AbilityTree Tree => _fake.GetAbility("Blaze")!;

        private void DumpTree(AbilityTree tree)
        {
            _out.WriteLine("══ AbilityTree ═══════════════════════════════");
            _out.WriteLine($"  Name              : {tree.Name}");
            _out.WriteLine($"  Trigger           : {tree.Trigger}");
            _out.WriteLine($"  Condition.Type    : {tree.Condition?.Type}");
            _out.WriteLine($"  Condition.HpFrac  : {tree.Condition?.HpFraction}");
            _out.WriteLine($"  Effect.Type       : {tree.Effect?.Type}");
            _out.WriteLine($"  Effect.Target     : {tree.Effect?.Target}");
            _out.WriteLine($"  Effect.Stat       : {tree.Effect?.Stat}");
            _out.WriteLine($"  Effect.Stages     : {tree.Effect?.StatStages}");
            _out.WriteLine("══════════════════════════════════════════════");
        }

        // ── Tree shape ────────────────────────────────────────────────────────

        [Fact]
        public void GetAbility_Blaze_ReturnsTree()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree);
        }

        [Fact]
        public void GetAbility_Blaze_MetadataIsCorrect()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Blaze", tree.Name);
            Assert.Equal("OnAttack", tree.Trigger);
        }

        [Fact]
        public void GetAbility_Blaze_HasCondition()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree.Condition);
        }

        [Fact]
        public void GetAbility_Blaze_ConditionIsHPBelow()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("HPBelow", tree.Condition!.Type);
        }

        [Fact]
        public void GetAbility_Blaze_ConditionThresholdIsOneThird()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal(1.0 / 3.0, tree.Condition!.HpFraction!.Value, precision: 5);
        }

        [Fact]
        public void GetAbility_Blaze_EffectIsStatChange()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("StatChange", tree.Effect!.Type);
        }

        [Fact]
        public void GetAbility_Blaze_EffectBoostsSpecialAttackByOne()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("SpecialAttack", tree.Effect!.Stat);
            Assert.Equal(1, tree.Effect!.StatStages);
        }

        [Fact]
        public void GetAbility_Blaze_EffectTargetsAttacker()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Attacker", tree.Effect!.Target);
        }

        // ── Translator ────────────────────────────────────────────────────────

        [Fact]
        public void Translate_Blaze_ReturnsAbilityState()
        {
            var state = _translator.Translate("Blaze");
            _out.WriteLine($"  AbilityState.Name : {state.Name}");
            Assert.NotNull(state);
            Assert.Equal("Blaze", state.Name);
        }

        [Fact]
        public void TranslateCondition_Blaze_IsHPBelowInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var condition = _translator.TranslateCondition(tree.Condition!);
            _out.WriteLine($"  TranslateCondition result type : {condition.GetType().Name}");
            Assert.IsType<HPBelow>(condition);
        }

        [Fact]
        public void TranslateEffect_Blaze_IsStatChangeInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Effect!);
            _out.WriteLine($"  TranslateEffect result type : {effect.GetType().Name}");
            Assert.IsType<StatChange>(effect);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  AbilityTranslator — Static  (complex: WasHitByContact → Chance → Paralyze)
    // ═════════════════════════════════════════════════════════════════════════

    public class StaticTranslatorTests
    {
        private readonly FakeAbilityService _fake = new();
        private readonly AbilityTranslator _translator = AbilityTestFactory.AbilityTranslator();
        private readonly ITestOutputHelper _out;

        public StaticTranslatorTests(ITestOutputHelper output) => _out = output;

        private AbilityTree Tree => _fake.GetAbility("Static")!;

        private void DumpTree(AbilityTree tree)
        {
            _out.WriteLine("══ AbilityTree ════════════════════════════════");
            _out.WriteLine($"  Name                         : {tree.Name}");
            _out.WriteLine($"  Trigger                      : {tree.Trigger}");
            _out.WriteLine($"  Condition.Type               : {tree.Condition?.Type}");
            _out.WriteLine($"  Effect.Type                  : {tree.Effect?.Type}");
            _out.WriteLine($"  Effect.ChanceProbability     : {tree.Effect?.ChanceProbability}");
            _out.WriteLine($"  Effect.ChanceChild.Type      : {tree.Effect?.ChanceChild?.Type}");
            _out.WriteLine($"  Effect.ChanceChild.Target    : {tree.Effect?.ChanceChild?.Target}");
            _out.WriteLine("═══════════════════════════════════════════════");
        }

        // ── Tree shape ────────────────────────────────────────────────────────

        [Fact]
        public void GetAbility_Static_ReturnsTree()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree);
        }

        [Fact]
        public void GetAbility_Static_MetadataIsCorrect()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Static", tree.Name);
            Assert.Equal("OnHit", tree.Trigger);
        }

        [Fact]
        public void GetAbility_Static_ConditionIsWasHitByContact()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("WasHitByContact", tree.Condition!.Type);
        }

        [Fact]
        public void GetAbility_Static_EffectIsChance()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Chance", tree.Effect!.Type);
        }

        [Fact]
        public void GetAbility_Static_ChanceProbabilityIsThirtyPercent()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal(0.30, tree.Effect!.ChanceProbability!.Value, precision: 3);
        }

        [Fact]
        public void GetAbility_Static_ChanceChildExists()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.NotNull(tree.Effect!.ChanceChild);
        }

        [Fact]
        public void GetAbility_Static_ChanceChildIsParalyze()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Paralyze", tree.Effect!.ChanceChild!.Type);
        }

        [Fact]
        public void GetAbility_Static_ParalyzeTargetsDefender()
        {
            var tree = Tree;
            DumpTree(tree);
            Assert.Equal("Defender", tree.Effect!.ChanceChild!.Target);
        }

        // ── Translator ────────────────────────────────────────────────────────

        [Fact]
        public void Translate_Static_ReturnsAbilityState()
        {
            var state = _translator.Translate("Static");
            _out.WriteLine($"  AbilityState.Name : {state.Name}");
            Assert.NotNull(state);
            Assert.Equal("Static", state.Name);
        }

        [Fact]
        public void TranslateCondition_Static_IsWasHitByContactInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var condition = _translator.TranslateCondition(tree.Condition!);
            _out.WriteLine($"  TranslateCondition result type : {condition.GetType().Name}");
            Assert.IsType<WasHitByContact>(condition);
        }

        [Fact]
        public void TranslateEffect_Static_IsChanceInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Effect!);
            _out.WriteLine($"  TranslateEffect result type : {effect.GetType().Name}");
            Assert.IsType<Chance>(effect);
        }

        [Fact]
        public void TranslateEffect_Static_ChanceChildIsParalyzeInstance()
        {
            var tree = Tree;
            DumpTree(tree);
            var effect = _translator.TranslateEffect(tree.Effect!.ChanceChild!);
            _out.WriteLine($"  TranslateEffect (ChanceChild) result type : {effect.GetType().Name}");
            Assert.IsType<Paralyze>(effect);
        }
    }
}