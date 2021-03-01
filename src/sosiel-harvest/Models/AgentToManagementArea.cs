/// Name: AgentToManagementArea.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentToManagementArea
    {
        public AgentToManagementArea()
        {
            ManagementAreas = new List<string>();
        }

        public string Agent { get; set; }
        public List<string> ManagementAreas { get; }

        public SiteSelectionMethod SiteSelectionMethod { get; set; }
    }
}