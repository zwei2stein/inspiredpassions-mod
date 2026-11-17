using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{
    public class InspiredPassionsTraitEvaluationExtension : DefModExtension
    {
        public const int LegacyUnset = int.MinValue;

        // default for trait
        public TraitEvaluation traitEvaluation = TraitEvaluation.UNSPECIFIED;

        public List<InspiredPassionsTraitDegreeEvaluation> degrees;

        // TODO: Complete removal in Q2, 2027
        // removed,patches still setting them get a clear error instead of a silent change.
        public int goodAboveDegree = LegacyUnset;
        public int badBelowDegree = LegacyUnset;
    }

    public class InspiredPassionsTraitDegreeEvaluation
    {
        public int degree;
        public TraitEvaluation traitEvaluation = TraitEvaluation.UNSPECIFIED;
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
            return getEvaluation(trait.def, trait.Degree);
        }

        // final evaluation: player override, then def/patch value.
        public static TraitEvaluation getEvaluation(TraitDef traitDef, int degree)
        {
            if (TraitEvaluationOverrides.TryGet(traitDef, degree, out var overridden))
                return overridden;

            return getDefEvaluation(traitDef, degree);
        }

        // def/patch value, ignores player overrides.
        public static TraitEvaluation getDefEvaluation(TraitDef traitDef, int degree)
        {
            var extension = traitDef.GetModExtension<InspiredPassionsTraitEvaluationExtension>();
            if (extension == null)
                return TraitEvaluation.UNSPECIFIED;

            if (extension.degrees != null)
            {
                foreach (var entry in extension.degrees)
                {
                    if (entry != null && entry.degree == degree)
                        return entry.traitEvaluation;
                }
            }

            return extension.traitEvaluation;
        }

        public static string LabelFor(TraitEvaluation evaluation)
        {
            return ("InspiredPassions_TraitEvaluation_" + evaluation).Translate();
        }

        public static string DescriptionFor(TraitEvaluation evaluation)
        {
            return ("InspiredPassions_TraitEvaluation_" + evaluation + "_desc").Translate();
        }
    }

    public static class TraitEvaluationValidationUtil
    {
        // removed fields: always a red error. degree list mistakes: dev mode warnings.
        public static void ValidateAll()
        {
            foreach (var traitDef in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                if (traitDef.modExtensions == null)
                    continue;

                var extensionCount = 0;
                foreach (var modExtension in traitDef.modExtensions)
                {
                    if (!(modExtension is InspiredPassionsTraitEvaluationExtension extension))
                        continue;

                    extensionCount++;

                    if (extension.goodAboveDegree != InspiredPassionsTraitEvaluationExtension.LegacyUnset
                        || extension.badBelowDegree != InspiredPassionsTraitEvaluationExtension.LegacyUnset)
                    {
                        Log.Error("[InspiredPassions] TraitDef " + traitDef.defName + " (" + ModNameOf(traitDef) + ") sets goodAboveDegree/badBelowDegree. "
                                  + "These fields were removed and are ignored. Use a <degrees> list instead. "
                                  + "Degrees without an entry now use traitEvaluation (" + extension.traitEvaluation + ").");
                    }

                    if (Prefs.DevMode)
                        ValidateDegrees(traitDef, extension);
                }

                if (Prefs.DevMode && extensionCount > 1)
                {
                    Log.Warning("[InspiredPassions] TraitDef " + traitDef.defName + " (" + ModNameOf(traitDef) + ") has "
                                + extensionCount + " InspiredPassionsTraitEvaluationExtension entries. Only the first one is used.");
                }
            }
        }

        private static void ValidateDegrees(TraitDef traitDef, InspiredPassionsTraitEvaluationExtension extension)
        {
            if (extension.degrees == null)
                return;

            for (var i = 0; i < extension.degrees.Count; i++)
            {
                var entry = extension.degrees[i];
                if (entry == null)
                    continue;

                if (!HasDegree(traitDef, entry.degree))
                    Log.Warning("[InspiredPassions] TraitDef " + traitDef.defName + " (" + ModNameOf(traitDef) + ") lists degree "
                                + entry.degree + " in <degrees>, but the trait has no such degree.");

                for (var j = 0; j < i; j++)
                {
                    if (extension.degrees[j] != null && extension.degrees[j].degree == entry.degree)
                    {
                        Log.Warning("[InspiredPassions] TraitDef " + traitDef.defName + " (" + ModNameOf(traitDef) + ") lists degree "
                                    + entry.degree + " in <degrees> more than once. Only the first entry is used.");
                        break;
                    }
                }
            }
        }

        private static bool HasDegree(TraitDef traitDef, int degree)
        {
            if (traitDef.degreeDatas == null)
                return false;

            foreach (var degreeData in traitDef.degreeDatas)
                if (degreeData.degree == degree)
                    return true;

            return false;
        }

        private static string ModNameOf(TraitDef traitDef)
        {
            return traitDef.modContentPack?.Name ?? "unknown mod";
        }
    }
}
