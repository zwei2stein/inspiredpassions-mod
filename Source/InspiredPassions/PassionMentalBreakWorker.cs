using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace InspiredPassions
{
    public class PassionMentalBreakWorker : MentalBreakWorker
    {
        public override float CommonalityFor(Pawn pawn, bool moodCaused = false)
        {
            if (!InspiredPassionsSettings.passionMetalBreakOn)
                return 0f;

            if (!PawnEligibilityUtil.IsEligible(pawn))
                return 0f;
            
            var commonality = base.CommonalityFor(pawn, moodCaused);

            var metrics = MetricsUtil.PassionMetricsFor(pawn);

            commonality *= (metrics.minorPassions + metrics.majorPassions) / (float)InspiredPassionsSettings.passionMaxCount;

            //Log.Message("[InspiredPassions] PassionMentalBreakWorker calculated commonality " + commonality + " " + pawn);

            return Math.Max(0.5f, commonality);
        }


        public override bool BreakCanOccur(Pawn pawn)
        {
            if (!InspiredPassionsSettings.passionMetalBreakOn)
                return false;

            if (!PawnEligibilityUtil.IsEligible(pawn))
                return false;
            
            if (!base.BreakCanOccur(pawn))
                return false;

            var metrics = MetricsUtil.PassionMetricsFor(pawn);

            // pawn has passions for all the enabled skills
            // and actually has passions to remove.
            if (metrics.enabledPassionableSkills == 0 && metrics.minorPassions + metrics.majorPassions > 0) return true;

            // pawn with few passions is ineligible, three passions is the worst childhood outcome.
            if (metrics.minorPassions + metrics.majorPassions <= 3) return false;

            //Log.Message("[InspiredPassions] legal target " + pawn);

            return true;
        }

        public override bool TryStart(Pawn pawn, string reason, bool causedByMood)
        {
            if (!PawnEligibilityUtil.IsEligible(pawn))
                return false;

            var candidatePassions = new List<PassionChange>(PassionUtils.GetMentalBreakPassionsToModify(pawn));

            if (candidatePassions.Count == 0)
            {
                //Log.Message("[InspiredPassions] no skill chosen.");
                return false;
            }

            var chosen = candidatePassions.RandomElementByWeight((Func<PassionChange, float>) (c => c.weight));

            var skillRecord = pawn.skills.GetSkill(chosen.skill);

            var reasonSuffix = "";
            if (reason != null)
                reasonSuffix = "\n\n" + reason;

            switch (chosen.desiredPassionGrade)
            {
                case Passion.None:
                    skillRecord.passion = Passion.None;
                    if (InspiredPassionsSettings.eventsGiveThoughts)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_LosePassion_Thought);
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionLostMinorLabel".Translate()
                            .Formatted(pawn.Named("PAWN"), chosen.skill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionLostMinor".Translate()
                            .Formatted(pawn.Named("PAWN"), chosen.skill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst() + reasonSuffix,
                        LetterDefOf.NegativeEvent,
                        (LookTargets)(Thing)pawn);
                    break;
                case Passion.Minor:
                    skillRecord.passion = Passion.Minor;
                    if (InspiredPassionsSettings.eventsGiveThoughts)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_LosePassion_Major_Thought);
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionLostMajorLabel".Translate()
                            .Formatted(pawn.Named("PAWN"), chosen.skill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionLostMajor".Translate()
                            .Formatted(pawn.Named("PAWN"), chosen.skill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst() + reasonSuffix,
                        LetterDefOf.NegativeEvent,
                        (LookTargets)(Thing)pawn);
                    break;
            }
            
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Catharsis);

            return true;
        }
    }
}