using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Landis.Extension.SOSIELHuman.Configuration
{
    using Enums;
    using Exceptions;


    /// <summary>
    /// Algorithm configuration model. Used to parse section "AlgorithmConfiguration".
    /// </summary>
    public class AlgorithmConfiguration
    {
        private CognitiveLevel cognitiveLevel;
        
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

    }
}
