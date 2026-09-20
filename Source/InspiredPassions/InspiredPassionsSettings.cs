using Verse;

namespace InspiredPassions
{

    public class InspiredPassionsSettings : ModSettings
    {
        public static bool passionInspirationOn = true;
        public static bool passionMetalBreakOn = true;

        public static bool traitInspirationOn = true;
        public static bool traitMetalBreakOn = true;
        
        public static bool visitorsOn = false;
        public static bool prisonersOn = true;
        public static bool slavesOn = true;

        public static float traitInspirationNeutralTraitsWeight = 0.5f;

        public static int traitMaxCount = 3;

        public static int passionMaxCount = 9;
        public static bool upgradeExistingPassions = true;

        public static bool eventsGiveThoughts = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref passionInspirationOn, "passionInspirationOn", true);
            Scribe_Values.Look(ref passionMetalBreakOn, "passionMetalBreakOn", true);

            Scribe_Values.Look(ref traitInspirationOn, "traitInspirationOn", true);
            Scribe_Values.Look(ref traitMetalBreakOn, "traitMetalBreakOn", true);
            
            Scribe_Values.Look(ref visitorsOn, "visitorsOn", false);
            Scribe_Values.Look(ref prisonersOn, "prisonersOn", true);
            Scribe_Values.Look(ref slavesOn, "slavesOn", true);

            Scribe_Values.Look(ref traitInspirationNeutralTraitsWeight, "traitInspirationNeutralTraitsWeight", 0.5f);

            Scribe_Values.Look(ref traitMaxCount, "traitMaxCount", 3);
            Scribe_Values.Look(ref passionMaxCount, "passionMaxCount", 9);

            Scribe_Values.Look(ref upgradeExistingPassions, "upgradeExistingPassions", true);

            Scribe_Values.Look(ref eventsGiveThoughts, "eventsGiveThoughts", true);

            base.ExposeData();
        }
    }
    
}