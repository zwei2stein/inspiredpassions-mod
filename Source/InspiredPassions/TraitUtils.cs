using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{
    public class TraitToModify
    {
        public TraitDef traitDef;
        public int degree = 0;
        public float weight = 0.5f;
        public TraitAction action;
    }

    public enum TraitAction
    {
        REMOVE,
        ADD
    }

    public class TraitUtils
    {
        public static IEnumerable<TraitToModify> GetInspirationTraitsToModify(Pawn pawn)
        {
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (trait.Suppressed)
                    continue;

                if (TraitEvaluationUtil.getEvaluation(trait) == TraitEvaluation.BAD)
                {
                    var commonality = trait.def.GetGenderSpecificCommonality(pawn.gender);
                    yield return new TraitToModify
                    {
                        traitDef = trait.def,
                        degree = trait.Degree,
                        weight = commonality,
                        action = TraitAction.REMOVE
                    };
                }
            }

            foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
            {
                if (pawn.story.traits.HasTrait(traitDef))
                    continue;

                var extension = traitDef.GetModExtension<InspiredPassionsTraitEvaluationExtension>();
                if (extension == null)
                    continue;

                if (extension.traitEvaluation == TraitEvaluation.GOOD)
                {
                    if (traitDef.degreeDatas.Count > 1)
                    {
                        foreach (var degreeData in traitDef.degreeDatas)
                        {
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count,
                                action = TraitAction.ADD
                            };
                        }
                    }
                    else
                    {
                        yield return new TraitToModify
                        {
                            traitDef = traitDef,
                            degree = 0,
                            weight = traitDef.GetGenderSpecificCommonality(pawn.gender),
                            action = TraitAction.ADD
                        };
                    }
                }
                else if (extension.traitEvaluation == TraitEvaluation.NEUTRAL)
                {
                    if (traitDef.degreeDatas.Count > 1)
                    {
                        foreach (var degreeData in traitDef.degreeDatas)
                        {
                            var commonality = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count;
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = commonality * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight,
                                action = TraitAction.ADD
                            };
                        }
                    }
                    else
                    {
                        yield return new TraitToModify
                        {
                            traitDef = traitDef,
                            degree = 0,
                            weight = traitDef.GetGenderSpecificCommonality(pawn.gender) * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight,
                            action = TraitAction.ADD
                        };
                    }
                }
                else if (extension.traitEvaluation == TraitEvaluation.UNSPECIFIED)
                {
                    foreach (var degreeData in traitDef.degreeDatas)
                    {
                        if (degreeData.degree > extension.goodAboveDegree)
                        {
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count,
                                action = TraitAction.ADD
                            };
                        }
                    }
                }
            }
        }

        public static IEnumerable<TraitToModify> GetMentalBreakTraitsToModify(Pawn pawn)
        {
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (trait.Suppressed)
                    continue;

                var commonality = trait.def.GetGenderSpecificCommonality(pawn.gender);
                switch (TraitEvaluationUtil.getEvaluation(trait))
                {
                    case TraitEvaluation.GOOD:
                        yield return new TraitToModify
                        {
                            traitDef = trait.def,
                            degree = trait.Degree,
                            weight = commonality,
                            action = TraitAction.REMOVE
                        };
                        break;
                    case TraitEvaluation.NEUTRAL:
                        yield return new TraitToModify
                        {
                            traitDef = trait.def,
                            degree = trait.Degree,
                            weight = commonality * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight,
                            action = TraitAction.REMOVE
                        };
                        break;
                }
            }

            foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
            {
                if (pawn.story.traits.HasTrait(traitDef))
                    continue;

                var extension = traitDef.GetModExtension<InspiredPassionsTraitEvaluationExtension>();
                if (extension == null)
                    continue;

                if (extension.traitEvaluation == TraitEvaluation.BAD)
                {
                    if (traitDef.degreeDatas.Count > 1)
                    {
                        foreach (var degreeData in traitDef.degreeDatas)
                        {
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count,
                                action = TraitAction.ADD
                            };
                        }
                    }
                    else
                    {
                        yield return new TraitToModify
                        {
                            traitDef = traitDef,
                            degree = 0,
                            weight = traitDef.GetGenderSpecificCommonality(pawn.gender),
                            action = TraitAction.ADD
                        };
                    }
                }
                else if (extension.traitEvaluation == TraitEvaluation.NEUTRAL)
                {
                    if (traitDef.degreeDatas.Count > 1)
                    {
                        foreach (var degreeData in traitDef.degreeDatas)
                        {
                            var commonality = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count;
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = commonality * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight,
                                action = TraitAction.ADD
                            };
                        }
                    }
                    else
                    {
                        yield return new TraitToModify
                        {
                            traitDef = traitDef,
                            degree = 0,
                            weight = traitDef.GetGenderSpecificCommonality(pawn.gender) * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight,
                            action = TraitAction.ADD
                        };
                    }
                }
                else if (extension.traitEvaluation == TraitEvaluation.UNSPECIFIED)
                {
                    foreach (var degreeData in traitDef.degreeDatas)
                    {
                        if (degreeData.degree < extension.badBelowDegree)
                        {
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count,
                                action = TraitAction.ADD
                            };
                        }
                    }
                }
            }
        }
    }
}