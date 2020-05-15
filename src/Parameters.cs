// This file is part of the SOSIEL Harvest Extension (SHE) for LANDIS-II.

// Can use classes from the listed namespaces.
using Landis.Utilities;

namespace Landis.Extension.SOSIELHuman
{
    public class Parameters
    {
        private int timestep;
        private string inputJasonFile;

        //---------------------------------------------------------------------

        public int Timestep
        {
            get
            {
                return timestep;
            }
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                  "Timestep must be > or = 0");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for pathnames for input maps of land use.
        /// </summary>
        public string InputJson
        {
            get
            {
                return inputJasonFile;
            }
            set
            {
                inputJasonFile = value;
            }
        }

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
