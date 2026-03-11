using PokemonGame.Model.Domain.Battle;
using PokemonGame.Model.Domain.Move;
using PokemonGame.Services.Enums.PokemonEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Model.Model.Helper.BattleHelper
{
    public class CreateMoves
    {
        //used for tables later
        public BattleDomain battleDomain;
        public void CreateHardCodedMove()
        {
            new MoveDomain(
                "Tackle",
                PokemonType.Normal,
                new Attempt
                (
                    new Probability(0.45),
                    new FormulaDamage
                    (
                        new DefenderTarget(),
                        new Exactly(40)
                    ),
                    null,
                    null
                )
            );
            new MoveDomain(
                "Body Slam",
                PokemonType.Normal,
                new Attempt(
                    new Probability(1.0), // accuracy

                    new Sequence(
                        new List<IEffect>
                        {
                            new FormulaDamage
                            (
                                new DefenderTarget(),
                                new Exactly(85)
                            ),

                            new Conditional(
                                new Probability(0.3),
                                new Paralyze(new DefenderTarget())
                            )
                        }
                    ),

                    null, // onMiss

                    null  // after
                )
            );
            new MoveDomain(
                "triple kick",
                PokemonType.Fighting,
                new Casade
                ( 
                    new List<IAttampt>
                    {
                        new Attempt(
                            new Probability(0.4),
                            new FormulaDamage
                            (
                                new DefenderTarget(),
                                new Exactly(10)
                            ),
                            null,
                            null
                        ),
                        new Attempt(
                            new Probability(0.4),
                            new FormulaDamage
                            (
                                new DefenderTarget(),
                                new Exactly(10)
                            ),
                            null,
                            null
                        ),
                        new Attempt(
                            new Probability(0.4),
                            new FormulaDamage
                            (
                                new DefenderTarget(),
                                new Exactly(10)
                            ),
                            null,
                            null
                        )
                    }
                )
            );
            new MoveDomain(
                "jump kick",
                PokemonType.Fighting,
                new Attempt
                (
                    new Probability(0.4),
                    new FormulaDamage
                    (
                        new DefenderTarget(),
                        new Exactly(100)
                    ),
                    new CrashDamage
                    (
                        new AttackerTarget(),
                        new Product
                        (
                            new MaxHP(new AttackerTarget()),
                            new Exactly(0.5)
                        )
                    ),
                    null
                )

            );

        } 
    }
}
