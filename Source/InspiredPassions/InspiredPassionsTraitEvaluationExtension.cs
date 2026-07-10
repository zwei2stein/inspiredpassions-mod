using RimWorld;
using Verse;

namespace InspiredPassions
{
    public class InspiredPassionsTraitEvaluationExtension : DefModExtension
    {
        public TraitEvaluation traitEvaluation = TraitEvaluation.UNSPECIFIED;

        public int goodAboveDegree = 0;
        public int badBelowDegree = 0;
    }

    public enum TraitEvaluation : byte
    {
        UNSPECIFIED,
        GOOD,
        NEUTRAL,
        BAD,
        DO_NOT_GRANT_OR_REMOVE
    }

    public class TraitEvaluationUtil
    {
        public static TraitEvaluation getEvaluation(Trait trait)
        {
            var extension = trait.def.GetModExtension<InspiredPassionsTraitEvaluationExtension>();
            if (extension == null)
                return TraitEvaluation.DO_NOT_GRANT_OR_REMOVE;

            if (extension.traitEvaluation != TraitEvaluation.UNSPECIFIED)
                return extension.traitEvaluation;

            if (trait.Degree < extension.badBelowDegree)
                return TraitEvaluation.BAD;

            if (trait.Degree > extension.goodAboveDegree)
                return TraitEvaluation.GOOD;

            return TraitEvaluation.NEUTRAL;
        }
        
    }
}