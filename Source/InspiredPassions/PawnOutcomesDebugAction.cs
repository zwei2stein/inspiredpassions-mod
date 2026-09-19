using System.Collections.Generic;
using System.Text;
using LudeonTK;
using Verse;

namespace InspiredPassions
{
    public class PawnOutcomesDebugAction
    {
        public static string ToLogString(PawnOutcomesReport report)
        {
            var sb = new StringBuilder();

            sb.AppendLine("[InspiredPassions] MD:\n#Outcomes for " + report.pawn.LabelShortCap);

            sb.AppendLine("## Metrics");
            sb.AppendLine(" * Passion: " + report.passionMetrics);
            sb.AppendLine("  * Trait: " + report.traitMetrics);

            sb.AppendLine("## Commonality / CanOccur");
            sb.AppendLine(" * Passion break: " + report.passionBreakCommonality.ToString("F3") + " / " + report.passionBreakCanOccur);
            sb.AppendLine(" * Trait break: " + report.traitBreakCommonality.ToString("F3") + " / " + report.traitBreakCanOccur);
            sb.AppendLine(" * Passion inspiration: " + report.passionInspirationCommonality.ToString("F3") + " / " + report.passionInspirationCanOccur);
            sb.AppendLine(" * Trait inspiration: " + report.traitInspirationCommonality.ToString("F3") + " / " + report.traitInspirationCanOccur);

            AppendPassionChanges(sb, "## Inspiration: Passion", report.inspirationPassions);
            AppendPassionChanges(sb, "## Mental Break: Passion", report.mentalBreakPassions);
            AppendTraitChanges(sb, "## Inspiration: Trait", report.inspirationTraits);
            AppendTraitChanges(sb, "## Mental Break: Trait", report.mentalBreakTraits);

            return sb.ToString();
        }

        private static void AppendPassionChanges(StringBuilder sb, string header, List<PassionChange> changes)
        {
            sb.AppendLine(header);

            if (changes.Count == 0)
            {
                sb.AppendLine(" * N/A");
                return;
            }

            foreach (var change in changes)
                sb.AppendLine(" * " + change.skill.defName + " -> " + change.desiredPassionGrade + " (weight=" + change.weight.ToString("F2") + ")");
        }

        private static void AppendTraitChanges(StringBuilder sb, string header, List<TraitToModify> changes)
        {
            sb.AppendLine(header);

            if (changes.Count == 0)
            {
                sb.AppendLine("  N/A");
                return;
            }

            foreach (var change in changes)
                sb.AppendLine(" * " + change.action + " " + change.traitDef.defName + " degree=" + change.degree + " (weight=" + change.weight.ToString("F2") + ")");
        }

        [DebugAction("InspiredPassions", "Log pawn outcomes", actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void LogPawnOutcomes(Pawn pawn)
        {
            Log.Message(ToLogString(PawnOutcomesReportUtil.BuildFor(pawn)));
        }
    }
}