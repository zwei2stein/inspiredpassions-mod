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
            if (Prefs.DevMode)
                SkillToTraitLinkValidationUtil.ValidateAll();
            Log.Message("[InspiredPassions] loaded!");
        }
    }

    public class InspiredPassionsMod : Mod
    {
        private readonly string[] tabNames =
        {
            "InspiredPassionsSettings_Tabs_General",
            "InspiredPassionsSettings_Tabs_Debug"
        };

        private PawnTableDef outcomesTableDef;
        private PawnTable outcomesPawnTable;

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
                "InspiredPassionsSettings_traitInspirationNeutralTraitsWeight".Translate(
                    InspiredPassionsSettings.traitInspirationNeutralTraitsWeight.ToStringDecimalIfSmall().Named("CHANCE")
                    ),
                InspiredPassionsSettings.traitInspirationNeutralTraitsWeight, 0f, 1f, 0.5f,
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
            
            listingStandard.Label("InspiredPassionsSettings_affects_label".Translate());

            listingStandard.Indent(gapWidth);
            listingStandard.ColumnWidth -= gapWidth;

            listingStandard.CheckboxLabeled(
                "InspiredPassionsSettings_affects_guests".Translate(),
                ref InspiredPassionsSettings.visitorsOn,
                "InspiredPassionsSettings_affects_guests_tooltip".Translate());
            
            listingStandard.CheckboxLabeled(
                "InspiredPassionsSettings_affects_prisoners".Translate(),
                ref InspiredPassionsSettings.prisonersOn,
                "InspiredPassionsSettings_affects_prisoners_tooltip".Translate());

            if (ModLister.IdeologyInstalled)
            {
                listingStandard.CheckboxLabeled(
                    "InspiredPassionsSettings_affects_slaves".Translate(),
                    ref InspiredPassionsSettings.slavesOn,
                    "InspiredPassionsSettings_affects_slaves_tooltip".Translate());
            }

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
            if (Find.CurrentMap == null)
            {
                Widgets.Label(rect, "InspiredPassions_OutcomesTable_GameNotRunning".Translate());
                return;
            }

            var pawns = Find.CurrentMap.mapPawns.FreeColonists;

            PawnOutcomesTableCache.Rebuild(pawns);

            if (outcomesTableDef == null)
                outcomesTableDef = BuildOutcomesTableDef();

            if (outcomesPawnTable == null)
                outcomesPawnTable = new PawnTable(
                    outcomesTableDef,
                    () => Find.CurrentMap.mapPawns.FreeColonists,
                    (int)rect.width,
                    (int)rect.height);

            outcomesPawnTable.SetMinMaxSize(outcomesTableDef.minWidth, (int)rect.width, 0, (int)rect.height);

            outcomesPawnTable.PawnTableOnGUI(new Vector2(rect.x, rect.y));
        }

        private static PawnTableDef BuildOutcomesTableDef()
        {
            return new PawnTableDef
            {
                defName = "InspiredPassions_OutcomesTable",
                minWidth = 700,
                columns = new List<PawnColumnDef>
                {
                    NewOutcomesColumn("IP_Name", typeof(PawnColumnWorker_IPName),
                        "InspiredPassionsSettings_Debug_Table_Name", null, sortable: true),

                    NewOutcomesColumn("IP_PassionBreakCommonality", typeof(PawnColumnWorker_IPPassionBreakCommonality),
                        "InspiredPassionsSettings_Debug_Table_Break_Passion",
                        "InspiredPassionsSettings_Debug_Table_Weight", sortable: true),
                    NewOutcomesColumn("IP_PassionBreakCanOccur", typeof(PawnColumnWorker_IPPassionBreakCanOccur),
                        null,
                        "InspiredPassionsSettings_Debug_Table_CanOccur"),

                    NewOutcomesColumn("IP_TraitBreakCommonality", typeof(PawnColumnWorker_IPTraitBreakCommonality),
                        "InspiredPassionsSettings_Debug_Table_Break_Traits",
                        "InspiredPassionsSettings_Debug_Table_Weight", sortable: true),
                    NewOutcomesColumn("IP_TraitBreakCanOccur", typeof(PawnColumnWorker_IPTraitBreakCanOccur),
                        null,
                        "InspiredPassionsSettings_Debug_Table_CanOccur"),

                    NewOutcomesColumn("IP_PassionInspirationCommonality", typeof(PawnColumnWorker_IPPassionInspirationCommonality),
                        "InspiredPassionsSettings_Debug_Table_Inspiration_Passion",
                        "InspiredPassionsSettings_Debug_Table_Weight", sortable: true),
                    NewOutcomesColumn("IP_PassionInspirationCanOccur", typeof(PawnColumnWorker_IPPassionInspirationCanOccur),
                        null,
                        "InspiredPassionsSettings_Debug_Table_CanOccur"),

                    NewOutcomesColumn("IP_TraitInspirationCommonality", typeof(PawnColumnWorker_IPTraitInspirationCommonality),
                        "InspiredPassionsSettings_Debug_Table_Inspiration_Traits",
                        "InspiredPassionsSettings_Debug_Table_Weight", sortable: true),
                    NewOutcomesColumn("IP_TraitInspirationCanOccur", typeof(PawnColumnWorker_IPTraitInspirationCanOccur),
                        null,
                        "InspiredPassionsSettings_Debug_Table_CanOccur"),

                    NewOutcomesColumn("IP_PassionMetrics", typeof(PawnColumnWorker_IPPassionMetrics),
                        "InspiredPassionsSettings_Debug_Table_Metrics_Passion",
                        "InspiredPassionsSettings_Debug_Table_Metrics_Passion_tooltip"),
                    NewOutcomesColumn("IP_TraitMetrics", typeof(PawnColumnWorker_IPTraitMetrics),
                        "InspiredPassionsSettings_Debug_Table_Metrics_Traits",
                        "InspiredPassionsSettings_Debug_Table_Metrics_Traits_tooltip"),
                }
            };
        }

        private static PawnColumnDef NewOutcomesColumn(string defName, System.Type workerClass, string labelKey, string tooltipKey, bool sortable = false)
        {
            return new PawnColumnDef
            {
                defName = defName,
                workerClass = workerClass,
                sortable = sortable,
                label = labelKey.NullOrEmpty() ? null : labelKey.Translate(),
                headerTip = tooltipKey.NullOrEmpty() ? null : tooltipKey.Translate()
            };
        }


        public override string SettingsCategory()
        {
            return "InspiredPassionsModName".Translate();
        }
    }
}