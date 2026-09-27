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