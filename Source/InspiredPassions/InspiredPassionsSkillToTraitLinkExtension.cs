using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{
    public class InspiredPassionsSkillToTraitLinkExtension : DefModExtension
    {
        
        public List<TraitDef> linkedTraits = new List<TraitDef>();
        public int degreeMin = 0;
        public int degreeMax = 0;

    }
}