using RimWorld;
using Verse;

namespace InspiredPassions
{
    public static class PawnEligibilityUtil
    {
        public static bool IsEligible(Pawn pawn)
        {
            if (pawn == null)
                return false;

            if (!pawn.RaceProps.Humanlike || pawn.story == null || pawn.skills == null || pawn.needs?.mood == null)
                return false;

            if (pawn.ageTracker.AgeBiologicalYears < Pawn_AgeTracker.MaxGrowthBirthday)
                return false;

            if (pawn.IsPrisonerOfColony)
                return InspiredPassionsSettings.prisonersOn;

            if (pawn.IsSlaveOfColony)
                return InspiredPassionsSettings.slavesOn;

            if (pawn.GuestStatus == GuestStatus.Guest)
                return InspiredPassionsSettings.visitorsOn;

            return pawn.Faction == Faction.OfPlayer;
        }
    }
}