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
            
            var commonality = base.CommonalityFor(pawn, moodCaused);

            var metrics = MetricsUtil.PassionMetricsFor(pawn);

            commonality *= (metrics.minorPassions + metrics.majorPassions) / MetricsUtil.SKILL_COUNT;

            //Log.Message("[InspiredPassions] PassionMentalBreakWorker calculated commonality " + commonality + " " + pawn);

            return commonality;
        }


        public override bool BreakCanOccur(Pawn pawn)
        {
            if (!InspiredPassionsSettings.passionMetalBreakOn)
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
            var candidateSkills = new List<SkillDef>();

            foreach (var skill in pawn.skills.skills)
            {
                if (skill.passion != Passion.None)
                {
                    candidateSkills.Add(skill.def);
                }
            }

            if (candidateSkills.Count == 0)
            {
                //Log.Message("[InspiredPassions] no skill chosen.");
                return false;
            }

            var chosenSkill = candidateSkills.RandomElement();

            var skillRecord = pawn.skills.GetSkill(chosenSkill);

            var reasonSuffix = "";
            if (reason != null)
                reasonSuffix = "\n\n" + reason;

            switch (skillRecord.passion)
            {
                case Passion.Minor:
                    skillRecord.passion = Passion.None;
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionLostMinorLabel".Translate()
                            .Formatted(pawn.Named("PAWN"), chosenSkill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionLostMinor".Translate()
                            .Formatted(pawn.Named("PAWN"), chosenSkill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst() + reasonSuffix,
                        LetterDefOf.NegativeEvent,
                        (LookTargets)(Thing)pawn);
                    break;
                case Passion.Major:
                    skillRecord.passion = Passion.Minor;
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionLostMajorLabel".Translate()
                            .Formatted(pawn.Named("PAWN"), chosenSkill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionLostMajor".Translate()
                            .Formatted(pawn.Named("PAWN"), chosenSkill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst() + reasonSuffix,
                        LetterDefOf.NegativeEvent,
                        (LookTargets)(Thing)pawn);
                    break;
                case Passion.None:
                default:
                    //Log.Message("[InspiredPassions] already not passion.");
                    return false;
            }

            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Catharsis);

            return true;
        }
    }
}