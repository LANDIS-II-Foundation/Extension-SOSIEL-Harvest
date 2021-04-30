/// Name: Mode3.cs
/// Description: 
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Linq;

using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Extension.SOSIELHarvest.Input;
using Landis.Library.HarvestManagement;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Mode3 : Mode
    {
        public Mode3(SheParameters sheParameters)
            : base(sheParameters)
        {
        }

        public override void Initialize()
        {
            var maDataSet = new ManagementAreaDataset();
            foreach (var agentToManagementArea in sheParameters.AgentToManagementAreaList)
            {
                foreach (var managementAreaName in agentToManagementArea.ManagementAreas)
                {
                    var managementArea = new ManagementArea(ushort.Parse(managementAreaName));
                    maDataSet.Add(managementArea);
                }
            }

            //ManagementAreas.ReadMap(sheParameters.ManagementAreaFileName, maDataSet);
            //Stands.ReadMap(sheParameters.StandsFileName);
            SiteVars.GetExternalVars();

            foreach (ManagementArea mgmtArea in maDataSet)
                mgmtArea.FinishInitialization();

            foreach (var agentToManagementArea in sheParameters.AgentToManagementAreaList)
            {
                foreach (var managementAreaName in agentToManagementArea.ManagementAreas)
                {
                    Area area;
                    if (!Areas.TryGetValue(managementAreaName, out area))
                    {
                        area = new Area();
                        area.Initialize(maDataSet.First(ma => ma.MapCode.ToString() == managementAreaName));
                        Areas.Add(managementAreaName, area);
                    }
                    area.AssignedAgents.Add(agentToManagementArea.Agent);
                }
            }
        }

        public override void Harvest(SosielData sosielData)
        {
            // Do not do anything here
            PlugIn.ModelCore.UI.WriteLine("Run Mode3 harvesting (no action)");
        }

        public override HarvestResults AnalyzeHarvestingResult()
        {
            // Report all zeros
            var results = new HarvestResults();
            foreach (var agent in Agents)
            {
                var areas = sheParameters.AgentToManagementAreaList
                    .First(map => map.Agent == agent.Id).ManagementAreas
                    .Select(ma => Areas.First(area => area.Key == ma).Value);
                foreach (var area in areas)
                {
                    var key = HarvestResults.GetKey(1, agent, area);
                    results.ManageAreaBiomass[key] = 0;
                    results.ManageAreaMaturityPercent[key] = 0;
                    results.ManageAreaBiomass[key] = 0;
                    results.ManageAreaHarvested[key] = 0;
                    results.ManageAreaMaturityPercent[key] = 0;
                }
            }
            return results;
        }
    }
}
