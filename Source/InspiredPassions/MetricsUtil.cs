using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace InspiredPassions
{
    public static class MetricsUtil
    {
        public static float SKILL_COUNT = 9f;
        
        private static int _lastCacheTick = -1;
        private static readonly Dictionary<Pawn, PassionMetrics> _passionMetricsCache = new Dictionary<Pawn, PassionMetrics>();
        private static readonly Dictionary<Pawn, TraitMetrics> _traitMetricsCache = new Dictionary<Pawn, TraitMetrics>();

        private static void ClearCachesIfNewTick()
        {
            if (Find.TickManager == null)
            {
                _passionMetricsCache.Clear();
                _traitMetricsCache.Clear();
                _lastCacheTick = -1;
                return;
            }

            int currentTick = Find.TickManager.TicksGame;
            if (currentTick != _lastCacheTick)
            {
                _passionMetricsCache.Clear();
                _traitMetricsCache.Clear();
                _lastCacheTick = currentTick;
            }
        }
        
        public static PassionMetrics PassionMetricsFor(Pawn pawn)
        {
            if (pawn == null)
                return new PassionMetrics();

            ClearCachesIfNewTick();

            if (_passionMetricsCache.TryGetValue(pawn, out var cachedMetrics))
                return cachedMetrics;

            var metrics = new PassionMetrics();

            foreach (var skill in pawn.skills.skills)
            {
                switch (skill.passion)
                {
                    case Passion.Minor:
                        metrics.minorPassions++;
                        if (!skill.TotallyDisabled)
                            metrics.enabledPassionableSkills++;
                        break;
                    case Passion.Major:
                        metrics.majorPassions++;
                        break;
                    case Passion.None:
                        metrics.nonePassions++;
                        if (!skill.TotallyDisabled)
                            metrics.enabledPassionableSkills++;
                        break;
                    default:
                        //Log.Message("[InspiredPassions] Unknown passion");
                        break;
                }
            }

            _passionMetricsCache[pawn] = metrics;
            return metrics;
        }
        
        public static TraitMetrics TraitMetricsFor(Pawn pawn)
        {
            if (pawn == null)
                return new TraitMetrics();

            ClearCachesIfNewTick();

            if (_traitMetricsCache.TryGetValue(pawn, out var cachedMetrics))
                return cachedMetrics;

            var metrics = new TraitMetrics();

            foreach (var trait in pawn.story.traits.allTraits)
            {

                if (ModsConfig.BiotechActive && trait.sourceGene != null)
                    continue;
                
                if (trait.Suppressed)
                    continue;

                switch (TraitEvaluationUtil.getEvaluation(trait))
                {
                    case TraitEvaluation.GOOD:
                        metrics.good++;
                        break;
                    case TraitEvaluation.BAD:
                        metrics.bad++;
                        break;
                    case TraitEvaluation.NEUTRAL:
                        metrics.neutral++;
                        break;
                    case TraitEvaluation.UNSPECIFIED:
                    case TraitEvaluation.DO_NOT_GRANT_OR_REMOVE:
                        metrics.doNotTouch++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _traitMetricsCache[pawn] = metrics;
            return metrics;
        }
        
    }
}