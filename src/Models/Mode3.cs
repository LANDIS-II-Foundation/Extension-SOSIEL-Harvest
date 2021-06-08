// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Linq;

using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Library.HarvestManagement;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class Mode3 : Mode
    {
        public Mode3(PlugIn plugin)
            : base(3, plugin)
        {
        }

        protected override void InitializeMode()
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

            Landis.Library.HarvestManagement.SiteVars.GetExternalVars();

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

        protected override void Harvest()
        {
            // Do not do anything here
            log.WriteLine("Mode 3: Harvesting (no action).");
        }

        protected override HarvestResults AnalyzeHarvestingResult()
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
                    var key = HarvestResults.GetKey(ModeId, agent, area);
                    results.ManagementAreaBiomass[key] = 0;
                    results.ManagementAreaMaturityPercent[key] = 0;
                    results.ManagementAreaBiomass[key] = 0;
                    results.ManagementAreaHarvested[key] = 0;
                    results.ManagementAreaMaturityPercent[key] = 0;
                }
            }
            return results;
        }
    }
}
