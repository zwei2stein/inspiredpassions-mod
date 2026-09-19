using System.Collections.Generic;
using Verse;

namespace InspiredPassions
{
    public class PawnOutcomesReport
    {
        public Pawn pawn;

        public PassionMetrics passionMetrics;
        public TraitMetrics traitMetrics;

        public float passionBreakCommonality;
        public bool passionBreakCanOccur;

        public float traitBreakCommonality;
        public bool traitBreakCanOccur;

        public float passionInspirationCommonality;
        public bool passionInspirationCanOccur;

        public float traitInspirationCommonality;
        public bool traitInspirationCanOccur;

        public List<PassionChange> inspirationPassions;
        public List<PassionChange> mentalBreakPassions;
        public List<TraitToModify> inspirationTraits;
        public List<TraitToModify> mentalBreakTraits;
    }

    public static class PawnOutcomesReportUtil
    {
        
        public static PawnOutcomesReport BuildFor(Pawn pawn)
        {
            var passionBreakWorker = new PassionMentalBreakWorker();
            passionBreakWorker.def = InspiredPassionsDefOf.InspiredPassions_LosePassion;

            var traitBreakWorker = new TraitMentalBreakWorker();
            traitBreakWorker.def = InspiredPassionsDefOf.InspiredPassions_LoseTrait;

            var passionInspirationWorker = new InspiredPassionWorker();
            passionInspirationWorker.def = InspiredPassionsDefOf.InspiredPassions_FindPassion;

            var traitInspirationWorker = new InspiredTraitWorker();
            traitInspirationWorker.def = InspiredPassionsDefOf.InspiredPassions_ImproveTraits;

            return new PawnOutcomesReport
            {
                pawn = pawn,

                passionMetrics = MetricsUtil.PassionMetricsFor(pawn),
                traitMetrics = MetricsUtil.TraitMetricsFor(pawn),

                passionBreakCommonality = passionBreakWorker.CommonalityFor(pawn),
                passionBreakCanOccur = passionBreakWorker.BreakCanOccur(pawn),

                traitBreakCommonality = traitBreakWorker.CommonalityFor(pawn),
                traitBreakCanOccur = traitBreakWorker.BreakCanOccur(pawn),

                passionInspirationCommonality = passionInspirationWorker.CommonalityFor(pawn),
                passionInspirationCanOccur = passionInspirationWorker.InspirationCanOccur(pawn),

                traitInspirationCommonality = traitInspirationWorker.CommonalityFor(pawn),
                traitInspirationCanOccur = traitInspirationWorker.InspirationCanOccur(pawn),

                inspirationPassions = new List<PassionChange>(PassionUtils.GetInspirationPassionsToModify(pawn)),
                mentalBreakPassions = new List<PassionChange>(PassionUtils.GetMentalBreakPassionsToModify(pawn)),
                inspirationTraits = new List<TraitToModify>(TraitUtils.GetInspirationTraitsToModify(pawn)),
                mentalBreakTraits = new List<TraitToModify>(TraitUtils.GetMentalBreakTraitsToModify(pawn))
            };
        }
    }
}