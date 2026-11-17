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
        
        // Ignore suppressed and gene-granted.
        public static bool isEligibleTrait(Trait trait)
        {
            if (trait.Suppressed)
                return false;

            if (ModsConfig.BiotechActive && trait.sourceGene != null)
                return false;

            return true;
        }

        
        public static bool canGainTrait(Pawn pawn, TraitDef traitDef)
        {
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (trait.def == traitDef)
                    return false; // includes gene-suppressed copy

                if (!trait.Suppressed && traitDef.ConflictsWith(trait))
                    return false;
            }

            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                foreach (var gene in pawn.genes.GenesListForReading)
                {
                    if (!gene.Active || gene.def.suppressedTraits == null)
                        continue;

                    foreach (var data in gene.def.suppressedTraits)
                    {
                        if (data.def == traitDef)
                            return false;
                    }
                }
            }

            return true;
        }


        public static IEnumerable<TraitToModify> GetInspirationTraitsToModify(Pawn pawn)
        {
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (!isEligibleTrait(trait))
                    continue;

                var commonality = trait.def.GetGenderSpecificCommonality(pawn.gender);
                switch (TraitEvaluationUtil.getEvaluation(trait))
                {
                    case TraitEvaluation.BAD:
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

            var metrics = MetricsUtil.TraitMetricsFor(pawn);
            if (metrics.good + metrics.neutral + metrics.bad + metrics.doNotTouch >= InspiredPassionsSettings.traitMaxCount)
                yield break;

            foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
            {
                if (!canGainTrait(pawn, traitDef))
                    continue;

                var commonality = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count;

                foreach (var degreeData in traitDef.degreeDatas)
                {
                    switch (TraitEvaluationUtil.getEvaluation(traitDef, degreeData.degree))
                    {
                        case TraitEvaluation.GOOD:
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = commonality,
                                action = TraitAction.ADD
                            };
                            break;
                        case TraitEvaluation.NEUTRAL:
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = commonality * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight,
                                action = TraitAction.ADD
                            };
                            break;
                    }
                }
            }
        }

        public static IEnumerable<TraitToModify> GetMentalBreakTraitsToModify(Pawn pawn)
        {
            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (!isEligibleTrait(trait))
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

            var metrics = MetricsUtil.TraitMetricsFor(pawn);
            if (metrics.good + metrics.neutral + metrics.bad + metrics.doNotTouch >= InspiredPassionsSettings.traitMaxCount)
                yield break;

            foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
            {
                if (!canGainTrait(pawn, traitDef))
                    continue;

                var commonality = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count;

                foreach (var degreeData in traitDef.degreeDatas)
                {
                    switch (TraitEvaluationUtil.getEvaluation(traitDef, degreeData.degree))
                    {
                        case TraitEvaluation.BAD:
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = commonality,
                                action = TraitAction.ADD
                            };
                            break;
                        case TraitEvaluation.NEUTRAL:
                            yield return new TraitToModify
                            {
                                traitDef = traitDef,
                                degree = degreeData.degree,
                                weight = commonality * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight,
                                action = TraitAction.ADD
                            };
                            break;
                    }
                }
            }
        }
    }
}