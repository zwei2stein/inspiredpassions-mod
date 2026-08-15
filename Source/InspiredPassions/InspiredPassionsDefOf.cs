using RimWorld;
using Verse;

namespace InspiredPassions
{
    [DefOf]
    public static class InspiredPassionsDefOf
    {
        public static ThoughtDef InspiredPassions_FindPassion_Thought;
        public static ThoughtDef InspiredPassions_FindPassion_Major_Thought;
        public static ThoughtDef InspiredPassions_ImproveTraits_Thought;
        public static ThoughtDef InspiredPassions_LosePassion_Thought;
        public static ThoughtDef InspiredPassions_LosePassion_Major_Thought;
        public static ThoughtDef InspiredPassions_LoseTrait_Thought;

        public static InspirationDef InspiredPassions_FindPassion;
        public static InspirationDef InspiredPassions_ImproveTraits;
        public static MentalBreakDef InspiredPassions_LosePassion;
        public static MentalBreakDef InspiredPassions_LoseTrait;

        static InspiredPassionsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(InspiredPassionsDefOf));
        }
    }
}