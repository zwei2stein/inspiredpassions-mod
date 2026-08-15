using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Noise;

namespace InspiredPassions
{
    public class InspiredTraitInspiration : Inspiration
    {
        public override void PostEnd()
        {
            base.PostEnd();

            var metrics = MetricsUtil.TraitMetricsFor(pawn);

            var removeBad = false;
            var addGood = false;

            if (metrics.bad > 0)
                removeBad = true;

            if (metrics.good + metrics.neutral + metrics.bad < InspiredPassionsSettings.traitMaxCount)
                addGood = true;

            Log.Message("[InspiredPassions] r" + removeBad + " a" + addGood);

            // we can both remove bad trait and add good trait, 
            if (removeBad && addGood)
                if (Rand.Bool)
                    removeBad = false;
                else
                    addGood = false;

            if (removeBad)
            {
                var candidateTraits = new List<TraitWithCommonality>();

                foreach (var trait in this.pawn.story.traits.allTraits)
                {
                    if (trait.Suppressed)
                        continue;

                    if (TraitEvaluationUtil.getEvaluation(trait) == TraitEvaluation.BAD)
                    {
                        var commonality = trait.def.GetGenderSpecificCommonality(pawn.gender);
                        candidateTraits.Add(new TraitWithCommonality(trait, commonality));
                    }
                }

                Log.Message(" " + candidateTraits.ToArray());

                if (candidateTraits.Count > 0)
                {
                    var removedTrait = candidateTraits.RandomElementByWeight<TraitWithCommonality>((Func<TraitWithCommonality, float>) (s => s.commonality));
                    pawn.story.traits.RemoveTrait(removedTrait.trait);
                    if (InspiredPassionsSettings.eventsGiveThoughts)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_ImproveTraits_Thought);

                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionTraitRemovedPositiveLabel".Translate()
                            .Formatted(this.pawn.Named("PAWN"), removedTrait.trait.CurrentData.label.Named("TRAIT"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionTraitRemovedPositive".Translate()
                            .Formatted(this.pawn.Named("PAWN"), removedTrait.trait.CurrentData.label.Named("TRAIT"))
                            .CapitalizeFirst(),
                        LetterDefOf.PositiveEvent,
                        (LookTargets)(Thing)this.pawn);
                }
            }

            if (addGood)
            {
                var candidateTraits = new List<TraitWithCommonality>();

                foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
                {
                    if (pawn.story.traits.HasTrait(traitDef))
                        continue;

                    //Log.Message(traitDef);

                    var extension = traitDef.GetModExtension<InspiredPassionsTraitEvaluationExtension>();
                    if (extension == null)
                        continue;

                    if (extension.traitEvaluation == TraitEvaluation.GOOD)
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
                            if (degreeData.degree > extension.goodAboveDegree)
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
                        pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_ImproveTraits_Thought);

                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionTraitGainedPositiveLabel".Translate()
                            .Formatted(this.pawn.Named("PAWN"), addedTrait.trait.CurrentData.label.Named("TRAIT"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionTraitGainedPositive".Translate()
                            .Formatted(this.pawn.Named("PAWN"), addedTrait.trait.CurrentData.label.Named("TRAIT"))
                            .CapitalizeFirst(),
                        LetterDefOf.PositiveEvent,
                        (LookTargets)(Thing)this.pawn);
                }
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