// Can use classes from the listed namespaces.
using Newtonsoft.Json;
using SOSIEL.Configuration;
using SOSIEL.Enums;
using SOSIEL.Exceptions;

/// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Configuration
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
/*
Copyright 2020 Garry Sotnik

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
