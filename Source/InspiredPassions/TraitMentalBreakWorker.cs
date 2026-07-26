using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace InspiredPassions
{
    public class TraitMentalBreakWorker : MentalBreakWorker
    {
        
        public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
        {
            if (!InspiredPassionsSettings.traitMetalBreakOn)
                return 0f;
            
            var commonality = base.CommonalityFor(pawn, moodCaused);

            var metrics = MetricsUtil.TraitMetricsFor(pawn);

            // no traits, pawn should have high chance of getting one
            if (metrics.good + metrics.neutral + metrics.bad == 0)
            {
                commonality *= InspiredPassionsSettings.traitMaxCount;  
            }
            else
            {
                float maxTraits = Math.Max(1f, InspiredPassionsSettings.traitMaxCount - metrics.doNotTouch);
                
                var freeSlots = maxTraits - Math.Max(metrics.good + metrics.neutral + metrics.bad, maxTraits);
                commonality *= (freeSlots + metrics.good + metrics.neutral) / maxTraits;
            }

            //Log.Message("[InspiredPassions] TraitMentalBreakWorker calculated commonality " + commonality + " " + pawn);

            return commonality;
        }
        
        public override bool BreakCanOccur(Pawn pawn)
        {
            if (!InspiredPassionsSettings.traitMetalBreakOn)
                return false;
            
            if (!base.BreakCanOccur(pawn))
                return false;

            var metrics = MetricsUtil.TraitMetricsFor(pawn);

            // no good/neutral traits to remove
            // not enough place to add bad traits
            if (metrics.good + metrics.neutral == 0 
                && metrics.good + metrics.neutral + metrics.doNotTouch + metrics.bad >= InspiredPassionsSettings.traitMaxCount )
                return false;

            return true;
        }

        public override bool TryStart(Pawn pawn, string reason, bool causedByMood)
        {

            var metrics = MetricsUtil.TraitMetricsFor(pawn);

            var removeGood = false;
            var addBad = false;

            if (metrics.good + metrics.neutral > 0)
                removeGood = true;

            if (metrics.good + metrics.neutral + metrics.bad < InspiredPassionsSettings.traitMaxCount)
                addBad = true;
            
            // we can both remove bad trait and add good trait, 
            if (removeGood && addBad)
                if (Rand.Bool)
                    removeGood = false;
                else
                    addBad = false;

            if (removeGood)
            {
                var candidateTraits = new List<TraitWithCommonality>();

                foreach (var trait in pawn.story.traits.allTraits)
                {
                    if (trait.Suppressed)
                        continue;

                    var commonality = trait.def.GetGenderSpecificCommonality(pawn.gender);
                    switch (TraitEvaluationUtil.getEvaluation(trait))
                    {
                        case TraitEvaluation.GOOD:
                            candidateTraits.Add(new TraitWithCommonality(trait, commonality));
                            break;
                        case TraitEvaluation.NEUTRAL:
                            candidateTraits.Add(new TraitWithCommonality(trait, commonality * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight));
                            break;
                    }
                }

                if (candidateTraits.Count > 0)
                {
                    var removedTrait = candidateTraits.RandomElementByWeight<TraitWithCommonality>((Func<TraitWithCommonality, float>) (s => s.commonality));
                    pawn.story.traits.RemoveTrait(removedTrait.trait);
                    if (InspiredPassionsSettings.eventsGiveThoughts)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_LoseTrait_Thought);

                    var reasonSuffix = "";
                    if (reason != null)
                        reasonSuffix = "\n\n" + reason;
                    
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionTraitRemovedNegativeLabel".Translate()
                            .Formatted(pawn.Named("PAWN"), removedTrait.trait.CurrentData.label.Named("TRAIT"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionTraitRemovedNegative".Translate()
                            .Formatted(pawn.Named("PAWN"), removedTrait.trait.CurrentData.label.Named("TRAIT"))
                            .CapitalizeFirst() + reasonSuffix,
                        LetterDefOf.NegativeEvent,
                        (LookTargets)(Thing)pawn);
                }
                
            }

            if (addBad)
            {
                 var candidateTraits = new List<TraitWithCommonality>();

                foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
                {
                    if (pawn.story.traits.HasTrait(traitDef))
                        continue;

                    Log.Message(traitDef);

                    var extension = traitDef.GetModExtension<InspiredPassionsTraitEvaluationExtension>();
                    if (extension == null)
                        continue;

                    if (extension.traitEvaluation == TraitEvaluation.BAD)
                    {
                        if (traitDef.degreeDatas.Count > 1)
                        {
                            foreach (var degreeData in traitDef.degreeDatas)
                            {
                                //Log.Message("adding candidate" + traitDef);
                                var commonality = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count;
                                candidateTraits.Add(new TraitWithCommonality(new Trait(traitDef, degreeData.degree),
                                    commonality));
                            }
                        }
                        else
                        {
                            //Log.Message("adding candidate" + traitDef);
                            candidateTraits.Add(new TraitWithCommonality(new Trait(traitDef), traitDef.GetGenderSpecificCommonality(pawn.gender)));
                        }
                    }
                    else if (extension.traitEvaluation == TraitEvaluation.NEUTRAL)
                    {
                        if (traitDef.degreeDatas.Count > 1)
                        {
                            foreach (var degreeData in traitDef.degreeDatas)
                            {
                                //Log.Message("adding candidate" + traitDef);
                                var commonality = traitDef.GetGenderSpecificCommonality(pawn.gender) / traitDef.degreeDatas.Count;
                                candidateTraits.Add(new TraitWithCommonality(new Trait(traitDef, degreeData.degree),
                                    commonality * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight));
                            }
                        }
                        else
                        {
                            //Log.Message("adding candidate" + traitDef);
                            candidateTraits.Add(new TraitWithCommonality(new Trait(traitDef), traitDef.GetGenderSpecificCommonality(pawn.gender) * InspiredPassionsSettings.traitInspirationNeutralTraitsWeight));
                        }
                    }
                    else if (extension.traitEvaluation == TraitEvaluation.UNSPECIFIED)
                    {
                        foreach (var degreeData in traitDef.degreeDatas)
                        {
                            if (degreeData.degree < extension.badBelowDegree)
                            {
                                //Log.Message("adding candidate" + traitDef);
                                candidateTraits.Add(new TraitWithCommonality(new Trait(traitDef, degreeData.degree),
                                    traitDef.GetGenderSpecificCommonality(pawn.gender) /  traitDef.degreeDatas.Count));
                            }
                        }
                    }
                }

                if (candidateTraits.Count > 0)
                {
                    var addedTrait = candidateTraits.RandomElementByWeight<TraitWithCommonality>((Func<TraitWithCommonality, float>) (s => s.commonality));
                    pawn.story.traits.GainTrait(addedTrait.trait);
                    if (InspiredPassionsSettings.eventsGiveThoughts)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_LoseTrait_Thought);
                    
                    var reasonSuffix = "";
                    if (reason != null)
                        reasonSuffix = "\n\n" + reason;


                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionTraitGainedNegativeLabel".Translate()
                            .Formatted(pawn.Named("PAWN"), addedTrait.trait.CurrentData.label.Named("TRAIT"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionTraitGainedNegative".Translate()
                            .Formatted(pawn.Named("PAWN"), addedTrait.trait.CurrentData.label.Named("TRAIT"))
                            .CapitalizeFirst() + reasonSuffix,
                        LetterDefOf.NegativeEvent,
                        (LookTargets)(Thing)pawn);
                }
            }
            
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Catharsis);

            return true;
        }

    }
}