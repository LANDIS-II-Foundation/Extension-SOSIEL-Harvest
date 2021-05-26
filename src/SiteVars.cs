// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHarvest
{
    public static class SiteVars
    {
        private static ISiteVar<ISiteCohorts> _biomassCohorts;

        public static void Initialize()
        {

            _biomassCohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");
            if (_biomassCohorts == null)
            {
                throw new System.ApplicationException(
                    "Biomass cohorts not found. Please double-check that this extension" +
                    " is compatible with your chosen succession extension.");
            }
        }

        /// <summary>
        /// Biomass cohorts at the each site.
        /// </summary>
        public static ISiteVar<ISiteCohorts> BiomassCohorts { get => _biomassCohorts; }
    }
}
