/// Name: ParameterParser.cs
/// Description:
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

// Can use classes from the listed namespaces.
using Landis.Utilities;

namespace Landis.Extension.SOSIELHuman
{
    /// <summary>
    /// A parser that reads the extension's input and output parameters from
    /// a text file.
    /// </summary>
    public class ParameterParser
        : TextParser<Parameters> //BasicParameterParser<Parameters>
    {

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get {
                return PlugIn.ExtensionName;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ParameterParser()//ISpeciesDataset speciesDataset)
            //: base(speciesDataset, false)
        {
        }

        //---------------------------------------------------------------------

        protected override Parameters Parse()
        {
            ReadLandisDataVar();

            Parameters parameters = new Parameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<string> inputJsonFile = new InputVar<string>("InputJson");
            ReadVar(inputJsonFile);
            parameters.InputJson = inputJsonFile.Value;

            return parameters;
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
