using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{
    public class InspiredTraitInspiration : Inspiration
    {
        public override void PostEnd()
        {
            base.PostEnd();

            var candidateTraits = new List<TraitToModify>(TraitUtils.GetInspirationTraitsToModify(pawn));

            if (candidateTraits.Count == 0)
                return;

            var chosen = candidateTraits.RandomElementByWeight((Func<TraitToModify, float>) (c => c.weight));

            if (chosen.action == TraitAction.REMOVE)
            {
                var removedTrait = pawn.story.traits.GetTrait(chosen.traitDef);
                pawn.story.traits.RemoveTrait(removedTrait);
                if (InspiredPassionsSettings.eventsGiveThoughts)
                    pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_ImproveTraits_Thought);

                Find.LetterStack.ReceiveLetter(
                    "Message_InspiredPassionTraitRemovedPositiveLabel".Translate()
                        .Formatted(this.pawn.Named("PAWN"), removedTrait.CurrentData.label.Named("TRAIT"))
                        .CapitalizeFirst(),
                    "Message_InspiredPassionTraitRemovedPositive".Translate()
                        .Formatted(this.pawn.Named("PAWN"), removedTrait.CurrentData.label.Named("TRAIT"))
                        .CapitalizeFirst(),
                    LetterDefOf.PositiveEvent,
                    (LookTargets)(Thing)this.pawn);
            }
            else if (chosen.action == TraitAction.ADD)
            {
                var addedTrait = new Trait(chosen.traitDef, chosen.degree);
                pawn.story.traits.GainTrait(addedTrait);
                if (InspiredPassionsSettings.eventsGiveThoughts)
                    pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_ImproveTraits_Thought);

                Find.LetterStack.ReceiveLetter(
                    "Message_InspiredPassionTraitGainedPositiveLabel".Translate()
                        .Formatted(this.pawn.Named("PAWN"), addedTrait.CurrentData.label.Named("TRAIT"))
                        .CapitalizeFirst(),
                    "Message_InspiredPassionTraitGainedPositive".Translate()
                        .Formatted(this.pawn.Named("PAWN"), addedTrait.CurrentData.label.Named("TRAIT"))
                        .CapitalizeFirst(),
                    LetterDefOf.PositiveEvent,
                    (LookTargets)(Thing)this.pawn);
            }
            else
            {
                Log.Warning("[InspiredPassions] Chosen inspiration trait with no action " + chosen.traitDef.defName);
            }
        }
    }

    public class InspiredTraitWorker : InspirationWorker
    {
        public override float CommonalityFor(Pawn pawn)
        {
            if (!InspiredPassionsSettings.traitInspirationOn)
                return 0f;
            
            var commonality = base.CommonalityFor(pawn);

            var metrics = MetricsUtil.TraitMetricsFor(pawn);
            
            var freeSlots = Math.Max(0, InspiredPassionsSettings.traitMaxCount - (metrics.good + metrics.neutral + metrics.bad));
            var removableTraits = metrics.neutral + metrics.bad;
            
            // terrible pawn (only bad traits and no good traits will have greater chance)
            commonality *= freeSlots + removableTraits;
            
            //Log.Message("[InspiredPassions] InspiredTraitInspiration calculated commonality " + commonality + " " +pawn);

            return Math.Max(0.5f, commonality);
        }

        public override bool InspirationCanOccur(Pawn pawn)
        {
            if (!InspiredPassionsSettings.traitInspirationOn)
                return false;
            
            if (!base.InspirationCanOccur(pawn))
                return false;

            var metrics = MetricsUtil.TraitMetricsFor(pawn);

            // no bad or neutral trait to remove
            // too many traits to add good/neutral
            if (metrics.bad + metrics.neutral == 0
                 && metrics.good + metrics.neutral + metrics.bad >= InspiredPassionsSettings.traitMaxCount)
                return false;

            //Log.Message("[InspiredPassions] pawn is legal target for inspired passion");

            return true;
        }
    }
}