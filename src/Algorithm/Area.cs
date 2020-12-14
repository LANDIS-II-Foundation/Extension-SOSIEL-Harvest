/// Name: Area.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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
