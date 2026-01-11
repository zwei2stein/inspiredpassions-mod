using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{
    public class InspiredPassionInspiration : Inspiration
    {
        public override void PostEnd()
        {
            base.PostEnd();

            var candidateSkills = new List<SkillDef>();

            foreach (var skill in this.pawn.skills.skills)
            {
                if (!skill.TotallyDisabled && skill.passion != Passion.Major)
                {
                    candidateSkills.Add(skill.def);

                    var skillToTraitLink = skill.def.GetModExtension<InspiredPassionsSkillToTraitLinkExtension>();
                    if (skillToTraitLink == null)
                        continue;

                    //add more of this skill to list to increase weight if pawn has linked trait
                    foreach (var trait in skillToTraitLink.linkedTraits)
                    {
                        if (this.pawn.story.traits.HasTrait(trait))
                        {
                            if (trait.degreeDatas.Count > 0)
                            {
                                var degree = this.pawn.story.traits.GetTrait(trait).Degree;
                                if (skillToTraitLink.degreeMin <= degree && degree <= skillToTraitLink.degreeMax)
                                {
                                    candidateSkills.Add(skill.def);
                                    //Log.Message("[InspiredPassions] bonus " + trait + "->" + skill.def);
                                }
                            }
                            else
                            {
                                candidateSkills.Add(skill.def);
                                //Log.Message("[InspiredPassions] bonus " + trait + "->" + skill.def);
                            }
                        }
                    }
                }
            }

            var chosenSkill = candidateSkills.RandomElement();

            if (chosenSkill == null)
            {
                //Log.Message("[InspiredPassions] no skill chosen.");
                return;
            }

            var skillRecord = this.pawn.skills.GetSkill(chosenSkill);

            switch (skillRecord.passion)
            {
                case Passion.None:
                    skillRecord.passion = Passion.Minor;
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionFoundMinorLabel".Translate()
                            .Formatted(this.pawn.Named("PAWN"), chosenSkill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        (TaggedString)"Message_InspiredPassionFoundMinor".Translate()
                            .Formatted(this.pawn.Named("PAWN"), chosenSkill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        this.def.beginLetterDef,
                        (LookTargets)(Thing)this.pawn);
                    break;
                case Passion.Minor:
                    skillRecord.passion = Passion.Major;
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionFoundMajorLabel".Translate()
                            .Formatted(this.pawn.Named("PAWN"), chosenSkill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        (TaggedString)"Message_InspiredPassionFoundMajor".Translate()
                            .Formatted(this.pawn.Named("PAWN"), chosenSkill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        this.def.beginLetterDef,
                        (LookTargets)(Thing)this.pawn);
                    break;
                case Passion.Major:
                default:
                    break;
            }
        }
    }

    public class InspiredPassionWorker : InspirationWorker
    {
        public override float CommonalityFor(Pawn pawn)
        {
            if (!InspiredPassionsSettings.passionInspirationOn)
                return 0f;
            
            var commonality = base.CommonalityFor(pawn);

            var metrics = Util.PassionMetricsFor(pawn);

            commonality *= 9f / (metrics.minorPassions + metrics.majorPassions);

            //Log.Message("[InspiredPassions] InspiredPassionInspiration calculated commonality " + commonality + " " + pawn);

            return commonality;
        }

        public override bool InspirationCanOccur(Pawn pawn)
        {
            if (!InspiredPassionsSettings.passionInspirationOn)
                return false;
            
            if (!base.InspirationCanOccur(pawn))
            {
                return false;
            }

            var metrics = Util.PassionMetricsFor(pawn);

            // pawn that already has 10 - 12 passions is too many passions to get inspiration 
            if (metrics.minorPassions + metrics.majorPassions >= 9) return false;

            // there must be at least two skills pawn can do that are not already their passion do to upgrade
            // aka, no guaranteed passion
            if (metrics.enabledPassionableSkills <= 1) return false;

            //Log.Message("[InspiredPassions] pawn is legal target for inspired passion");

            return true;
        }
    }
}