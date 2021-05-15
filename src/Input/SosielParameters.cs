// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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

        public List<AgentDecisionOption> AgentDecisionOptions { get; set; }
    }
}
