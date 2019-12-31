using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Landis.Core;
using Landis.Library.HarvestManagement;
using Landis.Library.SiteHarvest;
using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class PrescriptionExtension
    {
        public static Prescription Copy(this Prescription prescription)
        {
            var name = prescription.Name;

            var standRankingMethod = CopyStandRankingMethod(prescription.StandRankingMethod);

            var siteSelector = CopySiteSelector(prescription.SiteSelectionMethod);

            var minTimeSinceDamage = prescription.MinTimeSinceDamage;

            var preventEstablishment = prescription.PreventEstablishment;

            var cohortCutter = GetTypePrivateField<ICohortCutter>(prescription, "cohortCutter");
            var cohortCutterCopy = CopyCohortCutter(cohortCutter);

            Prescription prescriptionCopy;

            if (prescription is SingleRepeatHarvest singleRepeatHarvest)
            {
                var additionalCohortCutter =
                    GetTypePrivateField<ICohortCutter>(singleRepeatHarvest, "additionalCohortCutter");
                var additionalCohortCutterCopy = CopyCohortCutter(additionalCohortCutter);

                prescriptionCopy = new SingleRepeatHarvest(name, standRankingMethod, siteSelector, cohortCutterCopy, null,
                    additionalCohortCutterCopy, null, minTimeSinceDamage, preventEstablishment, singleRepeatHarvest.Interval);
            }
            else if (prescription is RepeatHarvest repeatHarvest)
            {
                prescriptionCopy = new RepeatHarvest(name, standRankingMethod, siteSelector, cohortCutterCopy, null,
                    minTimeSinceDamage, preventEstablishment, repeatHarvest.Interval, repeatHarvest.TimesToRepeat);
            }
            else
            {
                prescriptionCopy = new Prescription(name, standRankingMethod, siteSelector, cohortCutterCopy, null, minTimeSinceDamage, preventEstablishment);
            }

            return prescriptionCopy;
        }

        private static IStandRankingMethod CopyStandRankingMethod(IStandRankingMethod standRankingMethod)
        {
            IStandRankingMethod rankingMethodCopy;

            if (standRankingMethod is EconomicRank economicRank)
            {
                var rankTable = GetTypePrivateField<EconomicRankTable>(economicRank, "rankTable");
                var rankTableCopy = CopyEconomicRankTable(rankTable);
                rankingMethodCopy = new EconomicRank(rankTableCopy);
            }
            else if (standRankingMethod is MaxCohortAge)
            {
                rankingMethodCopy = new MaxCohortAge();
            }
            else if (standRankingMethod is RandomRank)
            {
                rankingMethodCopy = new RandomRank();
            }
            else if (standRankingMethod is RegulateAgesRank)
            {
                rankingMethodCopy = new RegulateAgesRank();
            }
            else if (standRankingMethod is FireRiskRank fireRiskRank)
            {
                var fireRiskTable = GetTypePrivateField<FireRiskTable>(fireRiskRank, "rankTable");
                var fireRiskTableCopy = CopyFireRiskTable(fireRiskTable);
                rankingMethodCopy = new FireRiskRank(fireRiskTableCopy);
            }
            else if (standRankingMethod is TimeSinceDisturbanceRank)
            {
                rankingMethodCopy = new TimeSinceDisturbanceRank();
            }
            else
            {
                throw new System.Exception();
            }

            foreach (var requirement in standRankingMethod.Requirements)
            {
                if (requirement is Presalvage preSalvage)
                {
                    var preSalvageYears = GetTypePrivateField<ushort>(preSalvage, "presalvYears");
                    rankingMethodCopy.AddRequirement(new Presalvage(preSalvageYears));
                }
                else if (requirement is MinimumAge minimumAge)
                {
                    var minAge = GetTypePrivateField<ushort>(minimumAge, "minAge");
                    rankingMethodCopy.AddRequirement(new MinimumAge(minAge));
                }
                else if (requirement is MaximumAge maximumAge)
                {
                    var maxAge = GetTypePrivateField<ushort>(maximumAge, "maxAge");
                    rankingMethodCopy.AddRequirement(new MaximumAge(maxAge));
                }
                else if (requirement is TimeSinceLastFire timeSinceLastFire)
                {
                    var timeSinceFire = GetTypePrivateField<ushort>(timeSinceLastFire, "timeSinceFire");
                    rankingMethodCopy.AddRequirement(new TimeSinceLastFire(timeSinceFire));
                }
                else if (requirement is TimeSinceLastWind timeSinceLastWind)
                {
                    var timeSinceWind = GetTypePrivateField<ushort>(timeSinceLastWind, "timeSinceWind");
                    rankingMethodCopy.AddRequirement(new TimeSinceLastWind(timeSinceWind));
                }
                else if (requirement is StandAdjacency standAdjacency)
                {
                    var time = GetTypePrivateField<ushort>(standAdjacency, "time");
                    var type = GetTypePrivateField<string>(standAdjacency, "type");
                    rankingMethodCopy.AddRequirement(new StandAdjacency(time, type, standAdjacency.SetAside));
                }
                else if (requirement is SpatialArrangement spatialArrangement)
                {
                    var minAge = GetTypePrivateField<ushort>(spatialArrangement, "minAge");
                    rankingMethodCopy.AddRequirement(new SpatialArrangement(minAge));
                }
                else if (requirement is MinTimeSinceLastHarvest minTimeSinceLastHarvest)
                {
                    var minTime = GetTypePrivateField<ushort>(minTimeSinceLastHarvest, "minTime");
                    rankingMethodCopy.AddRequirement(new MinTimeSinceLastHarvest(minTime));
                }
                else if (requirement is InclusionRequirement inclusionRequirement)
                {
                    var ruleList = GetTypePrivateField<List<InclusionRule>>(inclusionRequirement, "rule_list");
                    var ruleListCopy = CopyInclusionRuleList(ruleList);
                    rankingMethodCopy.AddRequirement(new InclusionRequirement(ruleListCopy));
                }
            }

            return rankingMethodCopy;
        }

        private static EconomicRankTable CopyEconomicRankTable(EconomicRankTable rankTable)
        {
            var tableCopy = new EconomicRankTable();

            foreach (var species in PlugIn.ModelCore.Species)
                tableCopy[species] = new EconomicRankParameters(rankTable[species].Rank, rankTable[species].MinimumAge);

            return tableCopy;
        }

        private static FireRiskTable CopyFireRiskTable(FireRiskTable fireRiskTable)
        {
            var tableCopy = new FireRiskTable();

            var parameters = GetTypePrivateField<FireRiskParameters[]>(fireRiskTable, "parameters");

            for (int i = 0; i < parameters.Length; i++)
                tableCopy[i] = new FireRiskParameters(parameters[i].Rank);

            return tableCopy;
        }

        private static List<InclusionRule> CopyInclusionRuleList(List<InclusionRule> ruleList)
        {
            var ruleListCopy = new List<InclusionRule>();

            foreach (var inclusionRule in ruleList)
            {
                var speciesList = new List<string>();

                foreach (var species in inclusionRule.SpeciesList)
                    speciesList.Add(species);

                ruleListCopy.Add(new InclusionRule(inclusionRule.InclusionType, inclusionRule.RuleAgeRange,
                    (inclusionRule.PercentOfCells * 100).ToString(CultureInfo.InvariantCulture) + "%", speciesList));
            }

            return ruleListCopy;
        }

        private static ISiteSelector CopySiteSelector(ISiteSelector siteSelector)
        {
            ISiteSelector siteSelectorCopy;

            if (siteSelector is CompleteStand)
            {
                siteSelectorCopy = new CompleteStand();
            }
            else if (siteSelector is PartialStandSpreading partialStandSpreading)
            {
                var minTargetSize = GetTypePrivateField<double>(partialStandSpreading, "minTargetSize");
                var maxTargetSize = GetTypePrivateField<double>(partialStandSpreading, "maxTargetSize");
                siteSelectorCopy = new PartialStandSpreading(minTargetSize, maxTargetSize);
            }
            else if (siteSelector is CompleteStandSpreading completeStandSpreading)
            {
                var minTargetSize = GetTypePrivateField<double>(completeStandSpreading, "minTargetSize");
                var maxTargetSize = GetTypePrivateField<double>(completeStandSpreading, "maxTargetSize");
                siteSelectorCopy = new CompleteStandSpreading(minTargetSize, maxTargetSize);
            }
            else if (siteSelector is PatchCutting patchCutting)
            {
                var percent = GetTypePrivateField<double>(patchCutting, "percent");
                var patchSize = GetTypePrivateField<double>(patchCutting, "patch_size");
                var allowOverlap = GetTypePrivateField<bool>(patchCutting, "allowOverlap");
                siteSelectorCopy = new PatchCutting(new Percentage(percent), patchSize,
                    allowOverlap ? "AllowOverlap" : string.Empty);
            }
            else
            {
                throw new Exception();
            }

            return siteSelectorCopy;
        }

        private static ICohortCutter CopyCohortCutter(ICohortCutter cohortCutter)
        {
            ICohortSelector cohortSelectorCopy;

            if (cohortCutter.CohortSelector is ClearCut)
            {
                cohortSelectorCopy = new ClearCut();
            }
            else if (cohortCutter.CohortSelector is MultiSpeciesCohortSelector multiSpeciesCohortSelector)
            {
                var multiSpeciesCohortSelectorCopy = new MultiSpeciesCohortSelector();
                var selectionMethods =
                    GetTypePrivateField<Dictionary<ISpecies, SelectCohorts.Method>>(multiSpeciesCohortSelector,
                        "selectionMethods");

                foreach (var selectionMethod in selectionMethods)
                {
                    if (selectionMethod.Value.Method.DeclaringType == typeof(SpecificAgesCohortSelector))
                    {
                        var agesAndRanges =
                            GetTypePrivateField<AgesAndRanges>(selectionMethod.Value.Target, "agesAndRanges");
                        var ages = GetTypePrivateField<List<ushort>>(agesAndRanges, "ages");
                        var ranges = GetTypePrivateField<List<AgeRange>>(agesAndRanges, "ranges");

                        multiSpeciesCohortSelectorCopy[selectionMethod.Key] =
                            new SpecificAgesCohortSelector(new List<ushort>(ages), new List<AgeRange>(ranges))
                                .SelectCohorts;
                    }
                    else
                    {
                        multiSpeciesCohortSelectorCopy[selectionMethod.Key] = selectionMethod.Value;
                    }
                }

                cohortSelectorCopy = multiSpeciesCohortSelector;
            }
            else
            {
                throw new Exception();
            }

            return new WholeCohortCutter(cohortSelectorCopy, HarvestExtensionMain.ExtType);
        }

        private static T GetTypePrivateField<T>(object @object, string fieldName)
        {
            var type = @object.GetType();
            var field = type.GetField(fieldName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            return (T) field.GetValue(@object);
        }
    }
}