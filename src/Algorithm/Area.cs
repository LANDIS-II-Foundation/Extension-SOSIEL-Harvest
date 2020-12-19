/// Name: Area.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;
using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class Area : IDataSet
    {
        public Area()
        {
            AssignedAgents = new List<string>();
        }

        public string Name { get; set; }

        public List<string> AssignedAgents { get; set; }
    }
}