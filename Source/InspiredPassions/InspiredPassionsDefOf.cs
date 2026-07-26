using RimWorld;

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

        static InspiredPassionsDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(InspiredPassionsDefOf));
        }
    }
}