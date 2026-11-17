using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace InspiredPassions
{
    [StaticConstructorOnStartup]
    public class PerTraitSettingsTab
    {
        private static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting");
        private static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending");

        private const float RowHeight = 28f;
        private const float HeaderHeight = 30f;
        private const float ControlsHeight = 30f;
        private const float Gap = 6f;
        private const float CellPadding = 4f;
        private const float ScrollbarWidth = 16f;

        private const float EvaluationWidth = 140f;
        private const float OverriddenWidth = 90f;
        private const float DegreeWidth = 60f;
        private const float CommonalityWidth = 110f;

        private const float SearchLabelWidth = 70f;
        private const float SearchFieldWidth = 220f;
        private const float ResetButtonWidth = 280f;

        private const float IconSize = 24f;

        private enum SortColumn
        {
            Name,
            Evaluation,
            Overridden,
            DefName,
            Degree,
            Commonality
        }

        private class Row
        {
            public TraitDef traitDef;
            public int degree;
            public string label;
            public string description;
            public string defName;
            public float commonalityMale;
            public string commonalityText;
            public string commonalityTooltip;
        }

        private struct Columns
        {
            public Rect name;
            public Rect evaluation;
            public Rect overridden;
            public Rect defName;
            public Rect degree;
            public Rect commonality;
        }

        private List<Row> allRows;

        private SortColumn sortColumn = SortColumn.Name;
        private bool sortDescending;

        private string searchText = "";
        private Vector2 scrollPosition;

        public void Draw(Rect rect)
        {
            if (allRows == null)
                BuildRows();

            var y = rect.y;

            var explanation = BuildExplanation();

            var explanationHeight = Text.CalcHeight(explanation, rect.width);
            Widgets.Label(new Rect(rect.x, y, rect.width, explanationHeight), explanation);
            y += explanationHeight + Gap;

            DrawControls(new Rect(rect.x, y, rect.width, ControlsHeight));
            y += ControlsHeight + Gap;

            var shownRows = ShownRows();

            var headerRect = new Rect(rect.x, y, rect.width - ScrollbarWidth, HeaderHeight);
            DrawHeader(headerRect);
            y += HeaderHeight;

            var outRect = new Rect(rect.x, y, rect.width, rect.yMax - y);

            if (shownRows.Count == 0)
            {
                Widgets.Label(outRect, "InspiredPassions_PerTrait_NoRows".Translate());
                return;
            }

            var viewRect = new Rect(0f, 0f, outRect.width - ScrollbarWidth, shownRows.Count * RowHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            // draw only visible rows.
            var first = Mathf.Max(0, Mathf.FloorToInt(scrollPosition.y / RowHeight));
            var last = Mathf.Min(shownRows.Count - 1, Mathf.CeilToInt((scrollPosition.y + outRect.height) / RowHeight));

            for (var i = first; i <= last; i++)
                DrawRow(new Rect(0f, i * RowHeight, viewRect.width, RowHeight), shownRows[i], i);

            Widgets.EndScrollView();
        }

        private void DrawControls(Rect rect)
        {
            var x = rect.x;

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(x, rect.y, SearchLabelWidth, rect.height), "InspiredPassions_PerTrait_Search".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            x += SearchLabelWidth;

            var newSearchText = Widgets.TextField(new Rect(x, rect.y, SearchFieldWidth, rect.height), searchText);
            if (newSearchText != searchText)
            {
                searchText = newSearchText;
                scrollPosition = Vector2.zero;
            }

            var count = TraitEvaluationOverrides.Count;

            var resetRect = new Rect(rect.xMax - ResetButtonWidth, rect.y, ResetButtonWidth, rect.height);
            
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonText(resetRect, "InspiredPassions_PerTrait_Reset".Translate(count.Named("COUNT")), true, true, count > 0) && count > 0)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "InspiredPassions_PerTrait_ResetConfirm".Translate(),
                    TraitEvaluationOverrides.Clear,
                    destructive: true));
            }
        }

        private void DrawHeader(Rect rect)
        {
            var columns = LayoutColumns(rect);

            DrawHeaderCell(columns.name, "InspiredPassions_PerTrait_Column_Trait", SortColumn.Name, TextAnchor.MiddleLeft);
            DrawHeaderCell(columns.evaluation, "InspiredPassions_PerTrait_Column_Evaluation", SortColumn.Evaluation, TextAnchor.MiddleCenter);
            DrawHeaderCell(columns.overridden, "InspiredPassions_PerTrait_Column_Overridden", SortColumn.Overridden, TextAnchor.MiddleCenter);

            if (Prefs.DevMode)
            {
                DrawHeaderCell(columns.defName, "InspiredPassions_PerTrait_Column_DefName", SortColumn.DefName, TextAnchor.MiddleLeft);
                DrawHeaderCell(columns.degree, "InspiredPassions_PerTrait_Column_Degree", SortColumn.Degree, TextAnchor.MiddleCenter);
                DrawHeaderCell(columns.commonality, "InspiredPassions_PerTrait_Column_Commonality", SortColumn.Commonality, TextAnchor.MiddleCenter);
            }
        }

        private void DrawHeaderCell(Rect rect, string labelKey, SortColumn column, TextAnchor anchor)
        {
            var labelRect = new Rect(rect.x + CellPadding, rect.y, rect.width - CellPadding * 2f, rect.height);

            Text.Anchor = anchor;
            Widgets.Label(labelRect, labelKey.Translate().Resolve().Truncate(labelRect.width));
            Text.Anchor = TextAnchor.UpperLeft;

            if (sortColumn == column)
            {
                var image = sortDescending ? SortingDescendingIcon : SortingIcon;
                GUI.DrawTexture(new Rect(rect.xMax - image.width - 1f, rect.yMax - image.height - 1f, image.width, image.height), image);
            }

            if (Mouse.IsOver(rect))
            {
                Widgets.DrawHighlight(rect);
                var tip = (labelKey + "_tooltip").Translate().Resolve() + "\n\n" + "ClickToSortByThisColumn".Translate().Resolve();
                TooltipHandler.TipRegion(rect, (TipSignal) tip);
            }

            if (!Widgets.ButtonInvisible(rect))
                return;

            if (sortColumn == column)
            {
                sortDescending = !sortDescending;
            }
            else
            {
                sortColumn = column;
                sortDescending = false;
            }

            SoundDefOf.Tick_High.PlayOneShotOnCamera();
        }

        private void DrawRow(Rect rect, Row row, int index)
        {
            if (index % 2 == 1)
                Widgets.DrawAltRect(rect);

            Widgets.DrawHighlightIfMouseover(rect);

            var columns = LayoutColumns(rect);

            var evaluation = TraitEvaluationUtil.getEvaluation(row.traitDef, row.degree);
            var overridden = TraitEvaluationOverrides.IsOverridden(row.traitDef, row.degree);

            Text.Anchor = TextAnchor.MiddleLeft;
            var labelRect = Padded(columns.name);
            Widgets.Label(labelRect, row.label.Truncate(columns.name.width - CellPadding * 2f));
            TooltipHandler.TipRegion(labelRect, (TipSignal) row.description);
            Text.Anchor = TextAnchor.UpperLeft;

            var evaluationRect = new Rect(columns.evaluation.x + 2f, columns.evaluation.y + 2f, columns.evaluation.width - 4f, columns.evaluation.height - 4f);
            if (Widgets.ButtonText(evaluationRect, TraitEvaluationUtil.LabelFor(evaluation)))
                OpenEvaluationMenu(row);
            TooltipHandler.TipRegion(evaluationRect, (TipSignal) TraitEvaluationUtil.DescriptionFor(evaluation));

            if (overridden)
            {
                var iconRect = new Rect(
                    columns.overridden.x + (columns.overridden.width - IconSize) / 2f,
                    columns.overridden.y + (columns.overridden.height - IconSize) / 2f,
                    IconSize,
                    IconSize);
                GUI.DrawTexture(iconRect, Widgets.CheckboxOnTex);

                var defaultLabel = TraitEvaluationUtil.LabelFor(TraitEvaluationUtil.getDefEvaluation(row.traitDef, row.degree));
                TooltipHandler.TipRegion(columns.overridden, (TipSignal) "InspiredPassions_PerTrait_Overridden_tooltip".Translate(defaultLabel.Named("DEFAULT")).Resolve());
            }

            if (Prefs.DevMode)
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(Padded(columns.defName), row.defName.Truncate(columns.defName.width - CellPadding * 2f));

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(columns.degree, row.degree.ToString());
                Widgets.Label(columns.commonality, row.commonalityText);
                Text.Anchor = TextAnchor.UpperLeft;

                TooltipHandler.TipRegion(columns.commonality, (TipSignal) row.commonalityTooltip);
            }
        }

        private void OpenEvaluationMenu(Row row)
        {
            var defEvaluation = TraitEvaluationUtil.getDefEvaluation(row.traitDef, row.degree);

            var options = new List<FloatMenuOption>();
            foreach (TraitEvaluation evaluation in Enum.GetValues(typeof(TraitEvaluation)))
            {
                var label = TraitEvaluationUtil.LabelFor(evaluation);
                if (evaluation == defEvaluation)
                    label = "InspiredPassions_PerTrait_DefaultOption".Translate(label.Named("EVALUATION")).Resolve();

                var chosen = evaluation;
                options.Add(new FloatMenuOption(label, () => TraitEvaluationOverrides.Set(row.traitDef, row.degree, chosen)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static Columns LayoutColumns(Rect rect)
        {
            var fixedWidth = EvaluationWidth + OverriddenWidth;
            if (Prefs.DevMode)
                fixedWidth += DegreeWidth + CommonalityWidth;

            var flexibleWidth = Mathf.Max(0f, rect.width - fixedWidth);
            var nameWidth = Prefs.DevMode ? flexibleWidth * 0.5f : flexibleWidth;

            var columns = new Columns();
            var x = rect.x;

            columns.name = new Rect(x, rect.y, nameWidth, rect.height);
            x += nameWidth;

            columns.evaluation = new Rect(x, rect.y, EvaluationWidth, rect.height);
            x += EvaluationWidth;

            columns.overridden = new Rect(x, rect.y, OverriddenWidth, rect.height);
            x += OverriddenWidth;

            if (Prefs.DevMode)
            {
                var defNameWidth = flexibleWidth - nameWidth;

                columns.defName = new Rect(x, rect.y, defNameWidth, rect.height);
                x += defNameWidth;

                columns.degree = new Rect(x, rect.y, DegreeWidth, rect.height);
                x += DegreeWidth;

                columns.commonality = new Rect(x, rect.y, CommonalityWidth, rect.height);
            }

            return columns;
        }

        private static Rect Padded(Rect rect)
        {
            return new Rect(rect.x + CellPadding, rect.y, rect.width - CellPadding * 2f, rect.height);
        }

        private void BuildRows()
        {
            allRows = new List<Row>();

            foreach (var traitDef in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                if (traitDef.degreeDatas == null)
                    continue;

                var male = traitDef.GetGenderSpecificCommonality(Gender.Male);
                var female = traitDef.GetGenderSpecificCommonality(Gender.Female);

                var commonalityText = male == female
                    ? male.ToString("0.##")
                    : "InspiredPassions_PerTrait_CommonalityGendered".Translate(male.ToString("0.##").Named("MALE"), female.ToString("0.##").Named("FEMALE")).Resolve();

                foreach (var degreeData in traitDef.degreeDatas)
                {
                    allRows.Add(new Row
                    {
                        traitDef = traitDef,
                        degree = degreeData.degree,
                        label = degreeData.label.NullOrEmpty() ? traitDef.defName : degreeData.LabelCap,
                        description = degreeData.description.NullOrEmpty() ? traitDef.description : degreeData.description,
                        defName = traitDef.defName,
                        commonalityMale = male,
                        commonalityText = commonalityText,
                        commonalityTooltip = "InspiredPassions_PerTrait_Column_Commonality_cell_tooltip"
                            .Translate(degreeData.commonality.ToString("0.##").Named("DEGREE"), traitDef.degreeDatas.Count.Named("COUNT"))
                            .Resolve()
                    });
                }
            }
        }

        // search filter + sort, rebuilt on every draw.
        private List<Row> ShownRows()
        {
            var shownRows = new List<Row>();

            var search = searchText.Trim();
            foreach (var row in allRows)
            {
                if (search.Length == 0
                    || row.label.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                    || (Prefs.DevMode && row.defName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    shownRows.Add(row);
                }
            }

            shownRows.Sort(CompareRows);

            return shownRows;
        }

        private int CompareRows(Row a, Row b)
        {
            int result;
            switch (sortColumn)
            {
                case SortColumn.Evaluation:
                    result = TraitEvaluationUtil.getEvaluation(a.traitDef, a.degree).CompareTo(TraitEvaluationUtil.getEvaluation(b.traitDef, b.degree));
                    break;
                case SortColumn.Overridden:
                    result = TraitEvaluationOverrides.IsOverridden(a.traitDef, a.degree).CompareTo(TraitEvaluationOverrides.IsOverridden(b.traitDef, b.degree));
                    break;
                case SortColumn.DefName:
                    result = string.Compare(a.defName, b.defName, StringComparison.OrdinalIgnoreCase);
                    break;
                case SortColumn.Degree:
                    result = a.degree.CompareTo(b.degree);
                    break;
                case SortColumn.Commonality:
                    result = a.commonalityMale.CompareTo(b.commonalityMale);
                    break;
                default:
                    result = string.Compare(a.label, b.label, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            if (sortDescending)
                result = -result;

            if (result != 0)
                return result;

            // stable order for ties.
            result = string.Compare(a.label, b.label, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
                return result;

            result = string.Compare(a.defName, b.defName, StringComparison.Ordinal);
            if (result != 0)
                return result;

            return a.degree.CompareTo(b.degree);
        }

        private static string BuildExplanation()
        {
            var text = new StringBuilder();

            foreach (TraitEvaluation evaluation in Enum.GetValues(typeof(TraitEvaluation)))
            {
                if (text.Length > 0)
                    text.Append("\n");

                text.Append("InspiredPassions_PerTrait_ExplanationLine".Translate(
                    TraitEvaluationUtil.LabelFor(evaluation).Named("LABEL"),
                    TraitEvaluationUtil.DescriptionFor(evaluation).Named("DESCRIPTION")).Resolve());
            }

            return text.ToString();
        }
    }
}
