using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{
    public class PassionChange
    {
        public SkillDef skill;
        public Passion desiredPassionGrade;
        public float weight;
    }
    
    public class PassionUtils
    {

        public static IEnumerable<PassionChange> GetInspirationPassionsToModify(Pawn pawn)
        {
            foreach (var skill in pawn.skills.skills)
            {
                if (!skill.TotallyDisabled && skill.passion != Passion.Major)
                {
                    if (!InspiredPassionsSettings.upgradeExistingPassions && skill.passion == Passion.Minor)
                        continue;

                    var weight = 1f;

                    var skillToTraitLink = skill.def.GetModExtension<InspiredPassionsSkillToTraitLinkExtension>();
                    if (skillToTraitLink != null)
                    {
                        foreach (var trait in skillToTraitLink.linkedTraits)
                        {
                            if (pawn.story.traits.HasTrait(trait))
                            {
                                if (trait.degreeDatas.Count > 0)
                                {
                                    var degree = pawn.story.traits.GetTrait(trait).Degree;
                                    if (skillToTraitLink.degreeMin <= degree && degree <= skillToTraitLink.degreeMax)
                                    {
                                        weight += 1f;
                                    }
                                }
                                else
                                {
                                    weight += 1f;
                                }
                            }
                        }
                    }

                    yield return new PassionChange
                    {
                        skill = skill.def,
                        desiredPassionGrade = skill.passion == Passion.None ? Passion.Minor : Passion.Major,
                        weight = weight
                    };
                }
            }
        }
        
        public static IEnumerable<PassionChange> GetMentalBreakPassionsToModify(Pawn pawn)
        {
            foreach (var skill in pawn.skills.skills)
            {
                if (skill.passion != Passion.None)
                {
                    yield return new PassionChange
                    {
                        skill = skill.def,
                        desiredPassionGrade = skill.passion == Passion.Minor ? Passion.None : Passion.Minor,
                        weight = 1f
                    };
                }
            }
        }
        
    }
}