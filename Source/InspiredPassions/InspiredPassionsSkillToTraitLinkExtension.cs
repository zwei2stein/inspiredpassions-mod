using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{

    public class InspiredPassionsLinkedTraitEntry
    {
        public TraitDef traitDef;
        public List<int> degreesActive;
    }

    public class InspiredPassionsSkillToTraitLinkExtension : DefModExtension
    {
        public List<InspiredPassionsLinkedTraitEntry> linkedTraits = new List<InspiredPassionsLinkedTraitEntry>();
    }

    public static class SkillToTraitLinkValidationUtil
    {
        public static void ValidateAll()
        {
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                var extension = skillDef.GetModExtension<InspiredPassionsSkillToTraitLinkExtension>();
                if (extension?.linkedTraits == null)
                    continue;

                foreach (var entry in extension.linkedTraits)
                {
                    var traitDef = entry.traitDef;

                    if (traitDef == null)
                        continue;

                    var hasMultipleDegrees = traitDef.degreeDatas != null && traitDef.degreeDatas.Count > 1;
                    var hasDegreesActive = entry.degreesActive != null && entry.degreesActive.Count > 0;

                    if (hasMultipleDegrees && !hasDegreesActive)
                    {
                        Log.Warning("[InspiredPassions] " + skillDef.defName + " links trait " + traitDef.defName +
                            ", has degrees, but no degreesActive.");
                    }
                    else if (!hasMultipleDegrees && hasDegreesActive)
                    {
                        Log.Warning("[InspiredPassions] " + skillDef.defName + " links trait " + traitDef.defName +
                            ", has degreesActive, but no degrees.");
                    }
                    else if (hasMultipleDegrees && hasDegreesActive)
                    {
                        foreach (var activeDegree in entry.degreesActive)
                        {
                            var matchesRealDegree = false;

                            foreach (var degreeData in traitDef.degreeDatas)
                            {
                                if (degreeData.degree == activeDegree)
                                {
                                    matchesRealDegree = true;
                                    break;
                                }
                            }

                            if (!matchesRealDegree)
                            {
                                Log.Warning("[InspiredPassions] " + skillDef.defName + " links trait " + traitDef.defName +
                                    " with degreesActive value " + activeDegree + ", it does not exist.");
                            }
                        }
                    }
                }
            }
        }
    }
}