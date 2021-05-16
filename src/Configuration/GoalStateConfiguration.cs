// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    /// <summary>
    /// Goal state configuration model. Used to parse section "InitialState.AgentsState.GoalsState".
    /// </summary>
    public class GoalStateConfiguration
    {
        [JsonRequired]
        public double Importance { get; set; }

        [JsonRequired]
        public double Value { get; set; }

        public double FocalValue { get; set; }

        public string FocalValueReference { get; set; }

        public double MinValue { get; set; }

        public double MaxValue { get; set; }

        public string MinValueReference { get; set; }

        public string MaxValueReference { get; set; }

        //public bool Randomness { get; set; }

        //public double RandomFrom { get; set; }

        //public double RandomTo { get; set; }

        //public string BasedOn { get; set; }
    }
}
