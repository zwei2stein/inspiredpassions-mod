using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace InspiredPassions
{
    public class InspiredPassionInspiration : Inspiration
    {
        public override void PostEnd()
        {
            base.PostEnd();

            if (!PawnEligibilityUtil.IsEligible(pawn))
                return;

            var candidatePassions = new List<PassionChange>(PassionUtils.GetInspirationPassionsToModify(pawn));

            if (candidatePassions.Count == 0)
            {
                //Log.Message("[InspiredPassions] no skill chosen.");
                return;
            }

            var chosen = candidatePassions.RandomElementByWeight((Func<PassionChange, float>) (c => c.weight));

            var skillRecord = this.pawn.skills.GetSkill(chosen.skill);

            switch (chosen.desiredPassionGrade)
            {
                case Passion.Minor:
                    skillRecord.passion = Passion.Minor;
                    if (InspiredPassionsSettings.eventsGiveThoughts)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_FindPassion_Thought);
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionFoundMinorLabel".Translate()
                            .Formatted(this.pawn.Named("PAWN"), chosen.skill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionFoundMinor".Translate()
                            .Formatted(this.pawn.Named("PAWN"), chosen.skill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        this.def.beginLetterDef,
                        (LookTargets)(Thing)this.pawn);
                    break;
                case Passion.Major:
                    skillRecord.passion = Passion.Major;
                    if (InspiredPassionsSettings.eventsGiveThoughts)
                        pawn.needs.mood.thoughts.memories.TryGainMemory(InspiredPassionsDefOf.InspiredPassions_FindPassion_Major_Thought);
                    Find.LetterStack.ReceiveLetter(
                        "Message_InspiredPassionFoundMajorLabel".Translate()
                            .Formatted(this.pawn.Named("PAWN"), chosen.skill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        "Message_InspiredPassionFoundMajor".Translate()
                            .Formatted(this.pawn.Named("PAWN"), chosen.skill.skillLabel.Named("SKILL"))
                            .CapitalizeFirst(),
                        this.def.beginLetterDef,
                        (LookTargets)(Thing)this.pawn);
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

            if (!PawnEligibilityUtil.IsEligible(pawn))
                return 0f;

            var commonality = base.CommonalityFor(pawn);

            var metrics = MetricsUtil.PassionMetricsFor(pawn);

            var existingPassions = Mathf.Max(1f, metrics.minorPassions + metrics.majorPassions);
            commonality *= InspiredPassionsSettings.passionMaxCount / existingPassions;

            return Math.Max(0.5f, commonality);
        }

        public override bool InspirationCanOccur(Pawn pawn)
        {
            if (!InspiredPassionsSettings.passionInspirationOn)
                return false;

            if (!PawnEligibilityUtil.IsEligible(pawn))
                return false;
            
            if (!base.InspirationCanOccur(pawn))
                return false;

            var metrics = MetricsUtil.PassionMetricsFor(pawn);

            // pawn that already has 10 - 12 passions is too many passions to get inspiration 
            if (metrics.minorPassions + metrics.majorPassions >= InspiredPassionsSettings.passionMaxCount) return false;
            
            // if turned off upgrading minors to major, we need to have enough unpassioned skills to have choice.
            if (!InspiredPassionsSettings.upgradeExistingPassions && metrics.nonePassions <= 1)
                return false;

            // there must be at least two skills pawn can do that are not already their passion do to upgrade
            // aka, no guaranteed passion
            if (metrics.enabledPassionableSkills <= 1) return false;

            //Log.Message("[InspiredPassions] pawn is legal target for inspired passion");

            return true;
        }
    }
}