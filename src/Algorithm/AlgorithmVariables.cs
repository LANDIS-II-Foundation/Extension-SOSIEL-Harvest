// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using SOSIEL.Helpers;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    /// <summary>
    /// Contains variable names used in code.
    /// </summary>
    public class AlgorithmVariables : SosielVariables
    {
        public const string ManageAreaHarvested = "ManageAreaHarvested";
        public const string ManageAreaMaturityPercent = "ManageAreaMaturityPercent";
        public const string ManageAreaBiomass = "ManageAreaBiomass";

        public const string Mean = "Mean";
        public const string StdDev = "StdDev";

        public const string Group = "Group";

        public const string TargetHarvestSize = "TargetHarvestSize";

        public const string AgentIncome = "Income";
        public const string AgentExpenses = "Expenses";
        public const string AgentSavings = "Savings";

        public const string HouseholdIncome = "HouseholdIncome";
        public const string HouseholdExpenses = "HouseholdExpenses";
        public const string HouseholdSavings = "HouseholdSavings";
    }
}
