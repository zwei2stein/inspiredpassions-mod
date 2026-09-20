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

            if (!PawnEligibilityUtil.IsEligible(pawn))
                return 0f;
            
            var commonality = base.CommonalityFor(pawn, moodCaused);

            var metrics = MetricsUtil.TraitMetricsFor(pawn);

            var freeSlots = Math.Max(0, InspiredPassionsSettings.traitMaxCount - (metrics.good + metrics.neutral + metrics.bad));
            var removableTraits = metrics.neutral + metrics.good;
            
            commonality *= freeSlots + removableTraits;

            //Log.Message("[InspiredPassions] TraitMentalBreakWorker calculated commonality " + commonality + " " + pawn);

            return Math.Max(0.5f, commonality);
        }
        
        public override bool BreakCanOccur(Pawn pawn)
        {
            if (!InspiredPassionsSettings.traitMetalBreakOn)
                return false;

            if (!PawnEligibilityUtil.IsEligible(pawn))
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
            if (!PawnEligibilityUtil.IsEligible(pawn))
                return false;

            var candidateTraits = new List<TraitToModify>(TraitUtils.GetMentalBreakTraitsToModify(pawn));

            if (candidateTraits.Count == 0)
                return false;

            var chosen = candidateTraits.RandomElementByWeight((Func<TraitToModify, float>) (c => c.weight));

            var reasonSuffix = "";
            if (reason != null)
                reasonSuffix = "\n\n" + reason;

            if (chosen.action == TraitAction.REMOVE)
            {
                var removedTrait = pawn.story.traits.GetTrait(chosen.traitDef);
                pawn.story.traits.RemoveTrait(removedTrait);
                if (InspiredPassionsSettings.eventsGiveThoughts)
                    pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_LoseTrait_Thought);

                Find.LetterStack.ReceiveLetter(
                    "Message_InspiredPassionTraitRemovedNegativeLabel".Translate()
                        .Formatted(pawn.Named("PAWN"), removedTrait.CurrentData.label.Named("TRAIT"))
                        .CapitalizeFirst(),
                    "Message_InspiredPassionTraitRemovedNegative".Translate()
                        .Formatted(pawn.Named("PAWN"), removedTrait.CurrentData.label.Named("TRAIT"))
                        .CapitalizeFirst() + reasonSuffix,
                    LetterDefOf.NegativeEvent,
                    (Thing)pawn);
            }
            else if (chosen.action == TraitAction.ADD)
            {
                var addedTrait = new Trait(chosen.traitDef, chosen.degree);
                pawn.story.traits.GainTrait(addedTrait);
                if (InspiredPassionsSettings.eventsGiveThoughts)
                    pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_LoseTrait_Thought);

                Find.LetterStack.ReceiveLetter(
                    "Message_InspiredPassionTraitGainedNegativeLabel".Translate()
                        .Formatted(pawn.Named("PAWN"), addedTrait.CurrentData.label.Named("TRAIT"))
                        .CapitalizeFirst(),
                    "Message_InspiredPassionTraitGainedNegative".Translate()
                        .Formatted(pawn.Named("PAWN"), addedTrait.CurrentData.label.Named("TRAIT"))
                        .CapitalizeFirst() + reasonSuffix,
                    LetterDefOf.NegativeEvent,
                    (Thing)pawn);
            }
            else
            {
                Log.Warning("[InspiredPassions] Chosen inspiration trait with no action " + chosen.traitDef.defName);
                return false;
            }

            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Catharsis);

            return true;
        }

    }
}