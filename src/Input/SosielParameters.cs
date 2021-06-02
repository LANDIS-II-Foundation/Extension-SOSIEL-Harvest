// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Collections.Generic;

using Landis.Extension.SOSIELHarvest.Models;

using SOSIEL.Enums;

namespace Landis.Extension.SOSIELHarvest.Input
{
    public class SosielParameters
    {
        public CognitiveLevel CognitiveLevel { get; set; }
        
        public Demographic Demographic { get; set; }

        public List<Probability> Probabilities { get; set; }

        public List<GoalAttribute> GoalAttributes { get; set; }

        public List<MentalModel> MentalModels { get; set; }

        public List<DecisionOptionAttribute> DecisionOptionAttributes { get; set; }

        public  List<DecisionOptionAntecedentAttribute> DecisionOptionAntecedentAttributes { get; set; }

        public List<AgentArchetype> AgentArchetypes { get; set; }

        public List<AgentArchetypeVariable> AgentArchetypeVariables { get; set; }

        public List<AgentGoalAttribute> AgentGoalAttributes { get; set; }

        public List<AgentVariable> AgentVariables { get; set; }

        public List<AgentDecisionOptions> AgentDecisionOptions { get; set; }
    }
}
