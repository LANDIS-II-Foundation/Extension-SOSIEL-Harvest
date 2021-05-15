// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHarvest.Configuration
{
    /// <summary>
    /// Probabilities configuration model. Used to parse section "ProbabilitiesConfiguration".
    /// </summary>
    public class ProbabilitiesConfiguration
    {
        [JsonRequired]
        public string Variable { get; set; }

        [JsonRequired]
        public string FilePath { get; set; }

        [JsonRequired]
        public string VariableType { get; set; }

        public bool WithHeader { get; set; }
    }
}
