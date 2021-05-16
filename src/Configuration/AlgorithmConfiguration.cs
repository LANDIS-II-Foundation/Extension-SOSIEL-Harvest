// Copyright (C) 2021 SOSIEL Inc. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using Newtonsoft.Json;

using SOSIEL.Configuration;
using SOSIEL.Enums;
using SOSIEL.Exceptions;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    /// <summary>
    /// Algorithm configuration model. Used to parse section "AlgorithmConfiguration".
    /// </summary>
    public class AlgorithmConfiguration
    {
        private CognitiveLevel cognitiveLevel;

        public bool UseDimographicProcesses { get; set; }

        [JsonRequired]
        public CognitiveLevel CognitiveLevel
        {
            get
            {
                return cognitiveLevel;
            }
            set
            {
                int val = (int)value;

                if (val < 1 && val > 4)
                    throw new InputParameterException(value.ToString(),
                        "CognitiveLevel must be in interval from 1 to 4");
                cognitiveLevel = value;
            }
        }

        public DemographicProcessesConfiguration DemographicConfiguration { get; set; }

        public ProbabilitiesConfiguration[] ProbabilitiesConfiguration { get; set; }
    }
}
