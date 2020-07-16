/// Name: SheParameters.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;
using Landis.Extension.SOSIELHarvest.Models;
using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest.Input
{
    public class SheParameters
    {
        private int _mode;
        private int _timestep;

        public int Mode
        {
            get => _mode;
            set
            {
                if (value != 1 && value != 2)
                    throw new InputValueException(value.ToString(), "Mode must be 1 or 2");

                _mode = value;
            }
        }

        public int Timestep
        {
            get => _timestep;
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Timestep must be > or = 0");
                _timestep = value;
            }
        }

        public string SosielInitializationFileName { get; set; }

        public string BiomassHarvestInitializationFileName { get; set; }

        public List<AgentToManagementArea> AgentToManagementAreaList { get; set; }
    }
}
