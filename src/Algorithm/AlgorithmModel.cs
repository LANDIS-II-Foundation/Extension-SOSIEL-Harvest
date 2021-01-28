/// Name: AlgorithmModel.cs
/// Description: The source code file used to pass external data.
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

﻿using System.Collections.Generic;
using Landis.Extension.SOSIELHarvest.Models;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    /// <summary>
    /// The model used to pass external data.
    /// </summary>
    public class AlgorithmModel
    {
        public int Mode { get; set; }

        public List<NewDecisionOptionModel> NewDecisionOptions { get; set; }

        // Management area to selected decision options list.
        public Dictionary<string, List<string>> SelectedDecisions { get; set; }

        public HarvestResults HarvestResults { get; set; }
    }
}
