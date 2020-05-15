// Can use classes from the listed namespaces.
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

using Newtonsoft.Json;

/// The container for classes and other namespaces.
namespace Landis.Extension.SOSIELHuman.Configuration
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

        public bool Randomness { get; set; }

        public double RandomFrom { get; set; }

        public double RandomTo { get; set; }

        public string BasedOn { get; set; }
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
