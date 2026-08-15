using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace InspiredPassions
{
    [StaticConstructorOnStartup]
    public static class InspiredPassionsMain
    {
        static InspiredPassionsMain()
        {
            Log.Message("[InspiredPassions] loaded!");
        }
    }

    public class InspiredPassionsSettings : ModSettings
    {
        public static bool passionInspirationOn = true;
        public static bool passionMetalBreakOn = true;

        public static bool traitInspirationOn = true;
        public static bool traitMetalBreakOn = true;

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

            Scribe_Values.Look(ref traitInspirationNeutralTraitsWeight, "traitInspirationNeutralTraitsWeight", 0.5f);

            Scribe_Values.Look(ref traitMaxCount, "traitMaxCount", 3);
            Scribe_Values.Look(ref passionMaxCount, "passionMaxCount", 9);

            Scribe_Values.Look(ref upgradeExistingPassions, "upgradeExistingPassions", true);

            Scribe_Values.Look(ref eventsGiveThoughts, "eventsGiveThoughts", true);

            base.ExposeData();
        }
    }

    public class InspiredPassionsMod : Mod
    {
        private readonly string[] tabNames =
        {
            "InspiredPassionsSettings_Tabs_General",
            "InspiredPassionsSettings_Tabs_Debug"
        };

        private Vector2 debugScrollPosition = Vector2.zero;

        private int selectedTab;
        private InspiredPassionsSettings settings;

        public InspiredPassionsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<InspiredPassionsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var tabHeight = 32f;
            
            const float topPadding = 25f;

            var tabRect = new Rect(
                inRect.x,
                inRect.y + topPadding,
                inRect.width,
                tabHeight
            );

            // Draw tabs
            var tabs = new List<TabRecord>();

            for (var i = 0; i < tabNames.Length; i++)
            {
                var tabIndex = i;

                tabs.Add(new TabRecord(
                    tabNames[i].Translate(),
                    () => selectedTab = tabIndex,
                    selectedTab == tabIndex
                ));
            }

            TabDrawer.DrawTabs(tabRect, tabs);

            // Content below tabs
            var contentRect = new Rect(
                inRect.x,
                inRect.y + tabHeight,
                inRect.width,
                inRect.height - topPadding - tabHeight
            );

            switch (selectedTab)
            {
                case 0:
                    DrawGeneralTab(contentRect);
                    break;

                case 1:
                    DrawDebugTab(contentRect);
                    break;
            }
        }

        private void DrawGeneralTab(Rect inRect)
        {
            var listingStandard = new Listing_Standard();

            var gapWidth = 12f;

            listingStandard.Begin(inRect);

            listingStandard.GapLine();

            listingStandard.Label("InspiredPassionsSettings_passions_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;

            listingStandard.CheckboxLabeled(
                "InspiredPassionsSettings_passionInspirationOn".Translate(),
                ref InspiredPassionsSettings.passionInspirationOn,
                "InspiredPassionsSettings_passionInspirationOn_tooltip".Translate());

            listingStandard.CheckboxLabeled(
                "InspiredPassionsSettings_passionMetalBreakOn".Translate(),
                ref InspiredPassionsSettings.passionMetalBreakOn,
                "InspiredPassionsSettings_passionMetalBreakOn_tooltip".Translate());

            InspiredPassionsSettings.passionMaxCount = Mathf.RoundToInt(listingStandard.SliderLabeled(
                "InspiredPassionsSettings_passionMaxCount".Translate(
                    InspiredPassionsSettings.passionMaxCount.Named("COUNT")),
                InspiredPassionsSettings.passionMaxCount, 1f, 12f, 0.5f,
                "InspiredPassionsSettings_passionMaxCount_tooltip".Translate()));

            listingStandard.CheckboxLabeled(
                "InspiredPassionsSettings_upgradeExistingPassions".Translate(),
                ref InspiredPassionsSettings.upgradeExistingPassions,
                "InspiredPassionsSettings_upgradeExistingPassions_tooltip".Translate());

            listingStandard.Outdent(gapWidth);
            listingStandard.ColumnWidth += gapWidth;

            listingStandard.GapLine();

            listingStandard.Label("InspiredPassionsSettings_traits_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;

            listingStandard.CheckboxLabeled(
                "InspiredPassionsSettings_traitInspirationOn".Translate(),
                ref InspiredPassionsSettings.traitInspirationOn,
                "InspiredPassionsSettings_traitInspirationOn_tooltip".Translate());

            listingStandard.CheckboxLabeled(
                "InspiredPassionsSettings_traitMetalBreakOn".Translate(),
                ref InspiredPassionsSettings.traitMetalBreakOn,
                "InspiredPassionsSettings_traitMetalBreakOn_tooltip".Translate());

            InspiredPassionsSettings.traitInspirationNeutralTraitsWeight = listingStandard.SliderLabeled(
                "InspiredPassionsSettings_traitInspirationNeutralTraitsWeight".Translate() +
                InspiredPassionsSettings.traitInspirationNeutralTraitsWeight.ToStringDecimalIfSmall()
                , InspiredPassionsSettings.traitInspirationNeutralTraitsWeight, 0f, 1f, 0.5f,
                "InspiredPassionsSettings_traitInspirationNeutralTraitsWeight_tooltip".Translate());

            InspiredPassionsSettings.traitMaxCount = Mathf.RoundToInt(listingStandard.SliderLabeled(
                "InspiredPassionsSettings_traitMaxCount".Translate(
                    InspiredPassionsSettings.traitMaxCount.Named("COUNT")),
                InspiredPassionsSettings.traitMaxCount, 1f, 6f, 0.5f,
                "InspiredPassionsSettings_traitMaxCount_tooltip".Translate()));

            listingStandard.Outdent(gapWidth);
            listingStandard.ColumnWidth += gapWidth;

            listingStandard.GapLine();

            listingStandard.Label("InspiredPassionsSettings_common_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;

            listingStandard.CheckboxLabeled(
                "InspiredPassionsSettings_eventsGiveThoughts".Translate(),
                ref InspiredPassionsSettings.eventsGiveThoughts,
                "InspiredPassionsSettings_eventsGiveThoughts_tooltip".Translate());

            listingStandard.Outdent(gapWidth);
            listingStandard.ColumnWidth += gapWidth;

            listingStandard.GapLine();

            if (Prefs.DevMode)
            {
                listingStandard.Label("InspiredPassionsSettings_devtools_section_label".Translate());

                listingStandard.Indent(gapWidth);
                listingStandard.ColumnWidth -= gapWidth;

                if (listingStandard.ButtonText("InspiredPassionsSettings_TraifDefCensus_button_label".Translate()))
                {
                    TraitDefsReportUtil.TraifDefCensus();
                    Find.WindowStack.Add(
                        new Dialog_MessageBox(
                            "InspiredPassionsSettings_TraifDefCensus_CSVCopied".Translate()
                        )
                    );
                }

                listingStandard.Outdent(gapWidth);
                listingStandard.ColumnWidth += gapWidth;

                listingStandard.GapLine();
            }

            listingStandard.End();

            base.DoSettingsWindowContents(inRect);
        }

        private void DrawDebugTab(Rect rect)
        {
            const float headerHeight = 30f;
            const float rowHeight = 28f;

            const float nameWidth = 130f;

            const float valueWidth = 85f;
            const float boolWidth = 30f;

            const float metricsWidth = 300f;

            var tableWidth =
                nameWidth +
                (valueWidth + boolWidth) * 4f +
                metricsWidth * 2f;

            var pawns = Find.CurrentMap.mapPawns.FreeColonists;

            var passionBreakWorker = new PassionMentalBreakWorker();
            passionBreakWorker.def = InspiredPassionsDefOf.InspiredPassions_LosePassion;

            var traitBreakWorker = new TraitMentalBreakWorker();
            traitBreakWorker.def = InspiredPassionsDefOf.InspiredPassions_LoseTrait;

            var passionInspirationWorker = new InspiredPassionWorker();
            passionInspirationWorker.def = InspiredPassionsDefOf.InspiredPassions_FindPassion;

            var traitInspirationWorker = new InspiredTraitWorker();
            traitInspirationWorker.def = InspiredPassionsDefOf.InspiredPassions_ImproveTraits;

            var viewRect = new Rect(
                0f,
                0f,
                tableWidth,
                headerHeight + pawns.Count * rowHeight
            );

            Widgets.BeginScrollView(
                rect,
                ref debugScrollPosition,
                viewRect
            );

            var y = 0f;
            float x;
            Rect cell;

            // =========================================================
            // Header
            // =========================================================

            x = 0f;

            cell = new Rect(x, y, nameWidth, headerHeight);
            Widgets.DrawBox(cell);
            Widgets.Label(cell.ContractedBy(5f), "InspiredPassionsSettings_Debug_Table_Name".Translate());
            x += nameWidth;

            // Passion break
            cell = new Rect(x, y, valueWidth + boolWidth, headerHeight);
            Widgets.DrawBox(cell);
            Widgets.Label(cell.ContractedBy(5f), "InspiredPassionsSettings_Debug_Table_Break_Passion".Translate());
            x += valueWidth + boolWidth;

            // Trait break
            cell = new Rect(x, y, valueWidth + boolWidth, headerHeight);
            Widgets.DrawBox(cell);
            Widgets.Label(cell.ContractedBy(5f), "InspiredPassionsSettings_Debug_Table_Break_Traits".Translate());
            x += valueWidth + boolWidth;

            // Passion inspiration
            cell = new Rect(x, y, valueWidth + boolWidth, headerHeight);
            Widgets.DrawBox(cell);
            Widgets.Label(cell.ContractedBy(5f), "InspiredPassionsSettings_Debug_Table_Inspiration_Passion".Translate());
            x += valueWidth + boolWidth;

            // Trait inspiration
            cell = new Rect(x, y, valueWidth + boolWidth, headerHeight);
            Widgets.DrawBox(cell);
            Widgets.Label(cell.ContractedBy(5f), "InspiredPassionsSettings_Debug_Table_Inspiration_Traits".Translate());
            x += valueWidth + boolWidth;

            // Passion metrics
            cell = new Rect(x, y, metricsWidth, headerHeight);
            Widgets.DrawBox(cell);
            Widgets.Label(cell.ContractedBy(5f), "InspiredPassionsSettings_Debug_Table_Metrics_Passion".Translate());
            x += metricsWidth;

            // Trait metrics
            cell = new Rect(x, y, metricsWidth, headerHeight);
            Widgets.DrawBox(cell);
            Widgets.Label(cell.ContractedBy(5f), "InspiredPassionsSettings_Debug_Table_Metrics_Traits".Translate());

            y += headerHeight;

            // =========================================================
            // Rows
            // =========================================================

            foreach (var pawn in pawns)
            {
                x = 0f;

                // Name
                cell = new Rect(x, y, nameWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    pawn.LabelShortCap
                );
                x += nameWidth;
                

                // Passion mental break
                var commonality =
                    passionBreakWorker.CommonalityFor(pawn);

                var canOccur =
                    passionBreakWorker.BreakCanOccur(pawn);

                cell = new Rect(x, y, valueWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    commonality.ToString("F3")
                );
                x += valueWidth;

                cell = new Rect(x, y, boolWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    canOccur ? "Y" : "N"
                );
                x += boolWidth;


                // Trait mental break
                commonality =
                    traitBreakWorker.CommonalityFor(pawn);

                canOccur =
                    traitBreakWorker.BreakCanOccur(pawn);

                cell = new Rect(x, y, valueWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    commonality.ToString("F3")
                );
                x += valueWidth;

                cell = new Rect(x, y, boolWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    canOccur ? "Y" : "N"
                );
                x += boolWidth;


                // Passion inspiration
                commonality =
                    passionInspirationWorker.CommonalityFor(pawn);

                var inspirationCanOccur =
                    passionInspirationWorker.InspirationCanOccur(pawn);

                cell = new Rect(x, y, valueWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    commonality.ToString("F3")
                );
                x += valueWidth;

                cell = new Rect(x, y, boolWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    inspirationCanOccur ? "Y" : "N"
                );
                x += boolWidth;


                // Trait inspiration
                commonality =
                    traitInspirationWorker.CommonalityFor(pawn);

                inspirationCanOccur =
                    traitInspirationWorker.InspirationCanOccur(pawn);

                cell = new Rect(x, y, valueWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    commonality.ToString("F3")
                );
                x += valueWidth;

                cell = new Rect(x, y, boolWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    inspirationCanOccur ? "Y" : "N"
                );
                x += boolWidth;


                // -----------------------------------------------------
                // Passion metrics
                // -----------------------------------------------------

                var passionMetrics =
                    MetricsUtil.PassionMetricsFor(pawn);

                cell = new Rect(x, y, metricsWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    passionMetrics?.ToString() ?? "None"
                );
                x += metricsWidth;


                // -----------------------------------------------------
                // Trait metrics
                // -----------------------------------------------------

                var traitMetrics =
                    MetricsUtil.TraitMetricsFor(pawn);

                cell = new Rect(x, y, metricsWidth, rowHeight);
                Widgets.DrawBox(cell);
                Widgets.Label(
                    cell.ContractedBy(5f),
                    traitMetrics?.ToString() ?? "None"
                );

                y += rowHeight;
            }

            Widgets.EndScrollView();
        }

        public override string SettingsCategory()
        {
            return "InspiredPassionsModName".Translate();
        }
    }

    public class TraitWithCommonality
    {
        public float commonality;

        public Trait trait;

        public TraitWithCommonality(Trait trait, float commonality)
        {
            this.trait = trait;
            this.commonality = commonality;
        }
    }
}