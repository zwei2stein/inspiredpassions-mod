using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{
    
    public class TraitEvaluationOverride : IExposable
    {
        public string traitDefName;
        public int degree;
        public TraitEvaluation traitEvaluation = TraitEvaluation.UNSPECIFIED;

        public void ExposeData()
        {
            Scribe_Values.Look(ref traitDefName, "traitDefName");
            Scribe_Values.Look(ref degree, "degree");
            Scribe_Values.Look(ref traitEvaluation, "traitEvaluation", TraitEvaluation.UNSPECIFIED);
        }
    }

    public static class TraitEvaluationOverrides
    {
        private static Dictionary<string, Dictionary<int, TraitEvaluation>> lookup;

        private static List<TraitEvaluationOverride> Overrides => InspiredPassionsSettings.traitEvaluationOverrides;

        public static int Count => Overrides.Count;

        public static bool TryGet(TraitDef traitDef, int degree, out TraitEvaluation evaluation)
        {
            if (lookup == null)
                RebuildLookup();

            if (lookup.TryGetValue(traitDef.defName, out var byDegree) && byDegree.TryGetValue(degree, out evaluation))
                return true;

            evaluation = TraitEvaluation.UNSPECIFIED;
            return false;
        }

        public static bool IsOverridden(TraitDef traitDef, int degree)
        {
            return TryGet(traitDef, degree, out _);
        }

        // picking the def/patch value removes the override.
        public static void Set(TraitDef traitDef, int degree, TraitEvaluation evaluation)
        {
            var overrides = Overrides;
            for (var i = overrides.Count - 1; i >= 0; i--)
            {
                if (overrides[i].traitDefName == traitDef.defName && overrides[i].degree == degree)
                    overrides.RemoveAt(i);
            }

            if (evaluation != TraitEvaluationUtil.getDefEvaluation(traitDef, degree))
            {
                overrides.Add(new TraitEvaluationOverride
                {
                    traitDefName = traitDef.defName,
                    degree = degree,
                    traitEvaluation = evaluation
                });
            }

            MarkDirty();
        }

        public static void Clear()
        {
            Overrides.Clear();
            MarkDirty();
        }

        public static void MarkDirty()
        {
            lookup = null;
            MetricsUtil.InvalidateCaches();
        }

        private static void RebuildLookup()
        {
            lookup = new Dictionary<string, Dictionary<int, TraitEvaluation>>();

            foreach (var entry in Overrides)
            {
                if (entry == null || entry.traitDefName.NullOrEmpty())
                    continue;

                if (!lookup.TryGetValue(entry.traitDefName, out var byDegree))
                {
                    byDegree = new Dictionary<int, TraitEvaluation>();
                    lookup[entry.traitDefName] = byDegree;
                }

                byDegree[entry.degree] = entry.traitEvaluation;
            }
        }
    }
}
