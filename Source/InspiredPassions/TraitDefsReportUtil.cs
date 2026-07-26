using System.Collections.Generic;
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

            csv.Append(
                "def_name;def_label;mod_package_id;mod_name;traitEvaluation;badBelowDegree;goodAboveDegree\n");

            foreach (var traitDef in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                
                csv.Append(traitDef.defName).Append(";");

                if (traitDef.degreeDatas != null)
                {
                    foreach (var degreeData in traitDef.degreeDatas)
                    {
                        csv.Append(degreeData.label.CapitalizeFirst());
                    }    
                }
                csv.Append(";");
                
                csv.Append(traitDef.modContentPack.PackageId).Append(";");
                csv.Append(traitDef.modContentPack.Name).Append(";");

                if (traitDef.HasModExtension<InspiredPassionsTraitEvaluationExtension>())
                {
                    var extension = traitDef.GetModExtension<InspiredPassionsTraitEvaluationExtension>();
                    csv.Append(extension.traitEvaluation).Append(";");
                    csv.Append(extension.badBelowDegree).Append(";");
                    csv.Append(extension.goodAboveDegree);
                }
                else
                {
                    csv.Append(";;");
                }
                
                csv.Append("\n");
                
            }

            GUIUtility.systemCopyBuffer = csv.ToString();
        }
    }
}