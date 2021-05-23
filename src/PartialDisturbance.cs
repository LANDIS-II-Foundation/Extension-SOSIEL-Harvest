// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

// Can use classes from the listed namespaces.
using Landis.Core;
using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHarvest
{
    /// <summary>
    /// A biomass disturbance that handles partial thinning of cohorts.
    /// </summary>
    public class PartialDisturbance
        : IDisturbance
    {
        private static PartialDisturbance singleton;

        private static ActiveSite currentSite;
        private static double fractionMortality = 0.0;  //assume same fraction applied to all cohorts

        //---------------------------------------------------------------------

        ActiveSite Landis.Library.BiomassCohorts.IDisturbance.CurrentSite
        {
            get
            {
                return currentSite;
            }
        }

        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get
            {
                return PlugIn.ExtType;
            }
        }

        //---------------------------------------------------------------------

        static PartialDisturbance()
        {
            singleton = new PartialDisturbance();
        }

        //---------------------------------------------------------------------

        public PartialDisturbance()
        {
        }

        //---------------------------------------------------------------------

        int IDisturbance.ReduceOrKillMarkedCohort(ICohort cohort)
        {
            int biomassMortality = 0;
            //double percentMortality = 0.0;
            int sppIndex = cohort.Species.Index;

            if (fractionMortality > 0.0)
            {
                biomassMortality += (int)((double)cohort.Biomass * fractionMortality);
                // PlugIn.ModelCore.UI.WriteLine("biomassMortality={0}, cohort.Biomass={1}, percentMortality={2:0.00}.", biomassMortality, cohort.Biomass, percentMortality);

            }

            if (biomassMortality > cohort.Biomass)
                biomassMortality = cohort.Biomass;

            //SiteVars.BiomassRemoved[currentSite] += biomassMortality;
            // PlugIn.ModelCore.UI.WriteLine("biomassMortality={0}, BiomassRemoved={1}.", biomassMortality, SiteVars.BiomassRemoved[currentSite]);

            if(biomassMortality > cohort.Biomass || biomassMortality < 0)
            {
                 PlugIn.ModelCore.UI.WriteLine("Cohort Total Mortality={0}. Cohort Biomass={1}. Site R/C={2}/{3}.", biomassMortality, cohort.Biomass, currentSite.Location.Row, currentSite.Location.Column);
                throw new System.ApplicationException("Error: Total Mortality is not between 0 and cohort biomass");
            }

            return biomassMortality;

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reduces the biomass of cohorts that have been marked for partial
        /// reduction.
        /// </summary>
        public static void ReduceCohortBiomass(ActiveSite site, double percentMortality)
        {
            currentSite = site;

            //PlugIn.ModelCore.UI.WriteLine("   ReduceCohortBiomass: Fraction Mortality = {0}, Percent Mortality = {1}", fractionMortality, percentMortality);
            fractionMortality = percentMortality;
            //PlugIn.ModelCore.UI.WriteLine("   ReduceCohortBiomass: Fraction Mortality = {0}, Percent Mortality = {1}", fractionMortality, percentMortality);

            SiteVars.Cohorts[site].ReduceOrKillBiomassCohorts(singleton);
        }
    }
}
