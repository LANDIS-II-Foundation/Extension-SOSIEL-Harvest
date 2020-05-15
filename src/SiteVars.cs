// This file is part of the SOSIEL Harvest Extension (SHE) for LANDIS-II.

// Can use classes from the listed namespaces.
using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;

namespace Landis.Extension.SOSIELHuman
{
    public class SiteVars
    {
        private static ISiteVar<ISiteCohorts> biomassCohorts;
        public static ISiteVar<string> PrescriptionName { get; set; }
        //---------------------------------------------------------------------

        public static void Initialize()
        {

            biomassCohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");

            if (biomassCohorts == null)
            {
                string mesg = string.Format("Cohorts are empty.  Please double-check that this extension is compatible with your chosen succession extension.");
                throw new System.ApplicationException(mesg);
            }

            PrescriptionName = PlugIn.ModelCore.Landscape.NewSiteVar<string>();
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.PrescriptionName, "Harvest.PrescriptionName");
            SiteVars.PrescriptionName.SiteValues = "SOSIEL";

        }

        // ----------------------------------------------------

        /// <summary>
        /// Biomass cohorts at each site.
        /// </summary>
        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return biomassCohorts;
            }
            set
            {
                biomassCohorts = value;
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
