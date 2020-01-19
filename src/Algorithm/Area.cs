using System.Collections.Generic;
using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class Area: IDataSet
    {
        public string Name { get; set; }

        public string[] AssignedAgents { get; set; }
    }
}