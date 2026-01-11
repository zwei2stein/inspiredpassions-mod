using System;
using RimWorld;
using Verse;
using UnityEngine;

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

    public class InspiredPassionsSettings: ModSettings
    {
        public static bool passionInspirationOn = true;
        public static bool passionMetalBreakOn = true;
        
        public static bool traitInspirationOn = true;
        public static bool traitMetalBreakOn = true;
        
        public static float traitInspirationNeutralTraitsWeight = 0.5f;

        public static int traitMaxCount = 3;
        
        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref passionInspirationOn, "passionInspirationOn", true);
            Scribe_Values.Look<bool>(ref passionMetalBreakOn, "passionMetalBreakOn", true);
            
            Scribe_Values.Look<bool>(ref traitInspirationOn, "traitInspirationOn", true);
            Scribe_Values.Look<bool>(ref traitMetalBreakOn, "traitMetalBreakOn", true);
            
            Scribe_Values.Look<float>(ref traitInspirationNeutralTraitsWeight, "traitInspirationNeutralTraitsWeight", 0.5f);
            
            Scribe_Values.Look<int>(ref traitMaxCount, "traitMaxCount", 3);

            base.ExposeData();
        }
    }
    
    public class InspiredPassionsMod : Mod
    {
        private InspiredPassionsSettings settings;

        public InspiredPassionsMod(ModContentPack content) : base(content)
        {
            this.settings = this.GetSettings<InspiredPassionsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
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
            
            listingStandard.End();
            
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "InspiredPassionsModName".Translate();
        }
    }

    public static class Util
    {
        public static PassionMetrics PassionMetricsFor(Pawn pawn)
        {
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
                        if (!skill.TotallyDisabled)
                            metrics.enabledPassionableSkills++;
                        break;
                    default:
                        //Log.Message("[InspiredPassions] Unknown passion");
                        break;
                }
            }

            //Log.Message("[InspiredPassions] metrics " + metrics.minorPassions + " " + metrics.majorPassions + " " + metrics.enabledPassionableSkills);

            return metrics;
        }
        
        
        public static TraitMetrics TraitMetricsFor(Pawn pawn)
        {
            var metrics = new TraitMetrics();

            foreach (var trait in pawn.story.traits.allTraits)
            {
                if (trait.Suppressed)
                    continue;
                
                switch (TraitEvaluationUtil.getEvalutation(trait))
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

            //Log.Message("[InspiredPassions] " + pawn + " TraitMetricsFor g" + metrics.good + " n" + metrics.neutral + " b" + metrics.bad + " nt" + metrics.doNotTouch);

            return metrics;
        }
        
    }

    public class PassionMetrics
    {
        public int minorPassions;
        public int majorPassions;
        public int enabledPassionableSkills;
    }
    
    public class TraitMetrics
    {
        public int good;
        public int neutral;
        public int bad;
        public int doNotTouch;
    }

    public class TraitWithCommonality
    {
        public TraitWithCommonality(Trait trait, float commonality)
        {
            this.trait = trait;
            this.commonality = commonality;
        }
        
        public Trait trait;
        public float commonality;
        
    }
}