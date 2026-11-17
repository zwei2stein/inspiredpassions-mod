using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace InspiredPassions
{
    public class TraitDefsReportUtil
    {
        
        public static void TraifDefCensus()
        {
            var csv = new StringBuilder();

            csv.Append("def_name;degree;degree_label;mod_package_id;mod_name;def_evaluation;final_evaluation;overridden\n");

            foreach (var traitDef in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                if (traitDef.degreeDatas == null)
                    continue;

                foreach (var degreeData in traitDef.degreeDatas)
                {
                    csv.Append(traitDef.defName).Append(";");
                    csv.Append(degreeData.degree).Append(";");
                    csv.Append(degreeData.label).Append(";");
                    csv.Append(traitDef.modContentPack?.PackageId).Append(";");
                    csv.Append(traitDef.modContentPack?.Name).Append(";");
                    csv.Append(TraitEvaluationUtil.getDefEvaluation(traitDef, degreeData.degree)).Append(";");
                    csv.Append(TraitEvaluationUtil.getEvaluation(traitDef, degreeData.degree)).Append(";");
                    csv.Append(TraitEvaluationOverrides.IsOverridden(traitDef, degreeData.degree) ? "yes" : "no");
                    csv.Append("\n");
                }
            }

            GUIUtility.systemCopyBuffer = csv.ToString();
        }
    }
}
