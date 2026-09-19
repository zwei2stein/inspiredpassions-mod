using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace InspiredPassions
{
    public static class PawnOutcomesTableCache
    {
        private static readonly Dictionary<Pawn, PawnOutcomesReport> reports = new Dictionary<Pawn, PawnOutcomesReport>();

        public static void Rebuild(IEnumerable<Pawn> pawns)
        {
            reports.Clear();

            foreach (var pawn in pawns)
                reports[pawn] = PawnOutcomesReportUtil.BuildFor(pawn);
        }

        public static PawnOutcomesReport ReportFor(Pawn pawn)
        {
            if (reports.TryGetValue(pawn, out var report))
                return report;
            
            var built = PawnOutcomesReportUtil.BuildFor(pawn);
            reports[pawn] = built;
            return built;
        }
    }

    public abstract class PawnColumnWorker_IPText : PawnColumnWorker
    {
        protected virtual TextAnchor Anchor => TextAnchor.MiddleCenter;
 
        protected abstract string GetTextFor(Pawn pawn);
 
        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            Text.Anchor = Anchor;
            Widgets.Label(rect.ContractedBy(5f), GetTextFor(pawn));
            Text.Anchor = TextAnchor.UpperLeft;
            
            if (Widgets.ButtonInvisible(rect))
            {
                Find.Selector.ClearSelection();
                Find.Selector.Select(pawn);
            }

        }
 
        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), WidestCellWidth(table));
        }
 
        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(WidestCellWidth(table), GetMinWidth(table), GetMaxWidth(table));
        }
 
        private int WidestCellWidth(PawnTable table)
        {
            var previousFont = Text.Font;
            Text.Font = GameFont.Small;
 
            var maxWidth = 0;
            foreach (var pawn in table.PawnsListForReading)
                maxWidth = Mathf.Max(maxWidth, Mathf.CeilToInt(Text.CalcSize(GetTextFor(pawn)).x));
 
            Text.Font = previousFont;
 
            return maxWidth + 10;
        }
    }
 
    public class PawnColumnWorker_IPName : PawnColumnWorker_IPText
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleLeft;
 
        protected override string GetTextFor(Pawn pawn) => pawn.LabelShortCap;
 
        public override int Compare(Pawn a, Pawn b)
        {
            return string.Compare(a.LabelShortCap, b.LabelShortCap, StringComparison.OrdinalIgnoreCase);
        }
    }
 
    public class PawnColumnWorker_IPPassionBreakCommonality : PawnColumnWorker_IPText
    {
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).passionBreakCommonality.ToString("F3");
 
        public override int Compare(Pawn a, Pawn b)
        {
            return PawnOutcomesTableCache.ReportFor(a).passionBreakCommonality.CompareTo(PawnOutcomesTableCache.ReportFor(b).passionBreakCommonality);
        }
    }
 
    public class PawnColumnWorker_IPPassionBreakCanOccur : PawnColumnWorker_IPText
    {
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).passionBreakCanOccur ? "Y" : "N";
    }
 
    public class PawnColumnWorker_IPTraitBreakCommonality : PawnColumnWorker_IPText
    {
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).traitBreakCommonality.ToString("F3");
 
        public override int Compare(Pawn a, Pawn b)
        {
            return PawnOutcomesTableCache.ReportFor(a).traitBreakCommonality.CompareTo(PawnOutcomesTableCache.ReportFor(b).traitBreakCommonality);
        }
    }
 
    public class PawnColumnWorker_IPTraitBreakCanOccur : PawnColumnWorker_IPText
    {
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).traitBreakCanOccur ? "Y" : "N";
    }
 
    public class PawnColumnWorker_IPPassionInspirationCommonality : PawnColumnWorker_IPText
    {
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).passionInspirationCommonality.ToString("F3");
 
        public override int Compare(Pawn a, Pawn b)
        {
            return PawnOutcomesTableCache.ReportFor(a).passionInspirationCommonality.CompareTo(PawnOutcomesTableCache.ReportFor(b).passionInspirationCommonality);
        }
    }
 
    public class PawnColumnWorker_IPPassionInspirationCanOccur : PawnColumnWorker_IPText
    {
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).passionInspirationCanOccur ? "Y" : "N";
    }
 
    public class PawnColumnWorker_IPTraitInspirationCommonality : PawnColumnWorker_IPText
    {
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).traitInspirationCommonality.ToString("F3");
 
        public override int Compare(Pawn a, Pawn b)
        {
            return PawnOutcomesTableCache.ReportFor(a).traitInspirationCommonality.CompareTo(PawnOutcomesTableCache.ReportFor(b).traitInspirationCommonality);
        }
    }
 
    public class PawnColumnWorker_IPTraitInspirationCanOccur : PawnColumnWorker_IPText
    {
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).traitInspirationCanOccur ? "Y" : "N";
    }
 
    public class PawnColumnWorker_IPPassionMetrics : PawnColumnWorker_IPText
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleLeft;
 
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).passionMetrics?.ToString() ?? "None";
    }
 
    public class PawnColumnWorker_IPTraitMetrics : PawnColumnWorker_IPText
    {
        protected override TextAnchor Anchor => TextAnchor.MiddleLeft;
 
        protected override string GetTextFor(Pawn pawn) => PawnOutcomesTableCache.ReportFor(pawn).traitMetrics?.ToString() ?? "None";
    }
    
}