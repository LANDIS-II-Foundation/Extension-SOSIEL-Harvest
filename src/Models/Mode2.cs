using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Helpers;
using Landis.Library.HarvestManagement;
using Landis.Utilities;
using HarvestManagement = Landis.Library.HarvestManagement;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Mode2 : Mode
    {
        private readonly BiomassHarvest.PlugIn _biomassHarvest;
        private readonly List<ExtendedPrescription> _extendedPrescriptions;

        public Mode2(BiomassHarvest.PlugIn biomassHarvest)
        {
            _biomassHarvest = biomassHarvest;
            _extendedPrescriptions = new List<ExtendedPrescription>();
        }

        public override void Initialize()
        {
            _biomassHarvest.Initialize();

            var biomassHarvestPluginType = _biomassHarvest.GetType();
            var managementAreasField = biomassHarvestPluginType.GetField("managementAreas",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (managementAreasField == null)
                throw new Exception();

            var managementAreas =
                ((IManagementAreaDataset) managementAreasField.GetValue(_biomassHarvest)).ToDictionary(
                    area => area.MapCode.ToString(), area => area);

            Areas = ((IManagementAreaDataset) managementAreasField.GetValue(_biomassHarvest)).ToDictionary(
                area => area.MapCode.ToString(), managementArea =>
                {
                    var area = new Area();
                    area.Initialize(managementArea);
                    return area;
                });

            foreach (var biomassHarvestArea in Areas.Values)
            {
                foreach (var appliedPrescription in biomassHarvestArea.ManagementArea.Prescriptions)
                    _extendedPrescriptions.Add(
                        appliedPrescription.ToExtendedPrescription(biomassHarvestArea.ManagementArea));
            }
        }

        public override void Harvest(SosielData sosielData)
        {
            //     _logService.WriteLine("Timestamp:\t" + ModelCore.CurrentTime);
            //
            //     if (_sheParameters.Mode == 2)
            //     {
            //         sosielHarvestModel.HarvestResults = AnalyzeHarvestResult();
            //         _logService.WriteLine("\tRun Sosiel with parameters:");
            //         foreach (var pair in sosielHarvestModel.HarvestResults.ManageAreaBiomass)
            //         {
            //             _logService.WriteLine(
            //                 $"\t\t{"Biomass:",-20}{sosielHarvestModel.HarvestResults.ManageAreaBiomass[pair.Key],10:N0}");
            //             _logService.WriteLine(
            //                 $"\t\t{"Harvested:",-20}{sosielHarvestModel.HarvestResults.ManageAreaHarvested[pair.Key],10:N0}");
            //             _logService.WriteLine(
            //                 $"\t\t{"MaturityPercent:",-20}{sosielHarvestModel.HarvestResults.ManageAreaMaturityPercent[pair.Key],10:F2}");
            //         }
            //
            //         var model = sosielHarvest.Run(sosielHarvestModel);
            //
            //         foreach (var decisionOptionModel in model.NewDecisionOptions)
            //         {
            //             GenerateNewPrescription(decisionOptionModel.Name, decisionOptionModel.ConsequentVariable,
            //                 decisionOptionModel.ConsequentValue, decisionOptionModel.BasedOn,
            //                 decisionOptionModel.ManagementArea);
            //         }
            //
            //         if (model.NewDecisionOptions.Any())
            //         {
            //             _logService.WriteLine("\tSosiel generated new prescriptions:");
            //             _logService.WriteLine($"\t\t{"Area",-10}{"Name",-20}{"Based on",-20}{"Variable",-40}{"Value",10}");
            //
            //             foreach (var decisionOption in model.NewDecisionOptions)
            //             {
            //                 _logService.WriteLine(
            //                     $"\t\t{decisionOption.ManagementArea,-10}{decisionOption.Name,-20}{decisionOption.BasedOn,-20}{decisionOption.ConsequentVariable,-40}{decisionOption.ConsequentValue,10}");
            //             }
            //         }
            //
            //         _logService.WriteLine($"\tSosiel selected the following prescriptions:");
            //         _logService.WriteLine($"\t\t{"Area",-10}Prescriptions");
            //
            //         foreach (var selectedDecisionPair in model.SelectedDecisions)
            //         {
            //             if (selectedDecisionPair.Value.Count == 0)
            //             {
            //                 _logService.WriteLine($"\t\t{selectedDecisionPair.Key,-10}none");
            //                 continue;
            //             }
            //
            //             var managementArea = _areas[uint.Parse(selectedDecisionPair.Key)];
            //
            //             managementArea.Prescriptions.RemoveAll(prescription =>
            //             {
            //                 var decisionPattern = new Regex(@"(MM\d+-\d+_DO\d+)");
            //                 return decisionPattern.IsMatch(prescription.Prescription.Name);
            //             });
            //
            //             foreach (var selectedDesignName in selectedDecisionPair.Value)
            //             {
            //                 var extendedPrescription =
            //                     _extendedPrescriptions.FirstOrDefault(ep =>
            //                         ep.ManagementArea.MapCode.Equals(managementArea.MapCode) &&
            //                         ep.Name.Equals(selectedDesignName));
            //                 if (extendedPrescription != null)
            //                     ApplyPrescription(managementArea, extendedPrescription);
            //             }
            //
            //             var prescriptionsLog = selectedDecisionPair.Value.Aggregate((s1, s2) => $"{s1} {s2}");
            //
            //             _logService.WriteLine($"\t\t{selectedDecisionPair.Key,-10}{prescriptionsLog}");
            //         }
            //
            //         _biomassHarvest.Run();
        }

        public override HarvestResults AnalyzeHarvestingResult()
        {
            throw new System.NotImplementedException();
        }

        private void GenerateNewPrescription(string newName, string parameter, dynamic value, string basedOn,
            string managementAreaName)
        {
            HarvestManagement.Prescription newPrescription;

            var area = Areas[managementAreaName];

            var appliedPrescription = area.ManagementArea.Prescriptions.First(p => p.Prescription.Name.Equals(basedOn));

            var areaToHarvest = appliedPrescription.PercentageToHarvest;
            var standsToHarvest = appliedPrescription.PercentStandsToHarvest;
            var beginTime = appliedPrescription.BeginTime;
            var endTime = appliedPrescription.EndTime;

            switch (parameter)
            {
                case "PercentOfHarvestArea":
                    var newAreaToHarvest = new Percentage(value / 100);
                    double cuttingMultiplier =
                        areaToHarvest.Value > 0 ? newAreaToHarvest.Value / areaToHarvest.Value : 1;
                    areaToHarvest = newAreaToHarvest;
                    newPrescription = appliedPrescription.Prescription.Copy(newName, cuttingMultiplier);
                    break;
                default:
                    throw new Exception();
            }

            _extendedPrescriptions.Add(new ExtendedPrescription(newPrescription, area.ManagementArea, areaToHarvest,
                standsToHarvest,
                beginTime, endTime));
        }

        private void ApplyPrescription(ManagementArea managementArea, ExtendedPrescription extendedPrescription)
        {
            managementArea.ApplyPrescription(extendedPrescription.Prescription,
                new Percentage(extendedPrescription.HarvestAreaPercent),
                new Percentage(extendedPrescription.HarvestStandsAreaPercent), extendedPrescription.StartTime,
                extendedPrescription.EndTime);
            managementArea.FinishInitialization();
        }
    }
}