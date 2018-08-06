// This file is part of the Social Human extension for LANDIS-II.

using Landis.SpatialModeling;

namespace Landis.Extension.SocialHuman
{
    /// <summary>
    /// A wrapper around the LandUse site variable that exposes the 
    /// AllowHarvest property as a read-only bool site variable.
    /// </summary>
    public class AllowHarvestSiteVar
        : ISiteVar<bool>
    {
        #region ISiteVariable members
        System.Type ISiteVariable.DataType
        {
            get
            {
                return typeof(bool);
            }
        }

        InactiveSiteMode ISiteVariable.Mode
        {
            get
            {
                return SiteVars.LandUse.Mode;
            }
        }

        ILandscape ISiteVariable.Landscape
        {
            get
            {
                return SiteVars.LandUse.Landscape;
            }
        }
        #endregion

        #region ISiteVar<bool> members
        // Other extensions only need read access.

        bool ISiteVar<bool>.this[Site site]
        {
            get
            {
                return SiteVars.LandUse[site].AllowHarvest;
            }
            set
            {
                throw new System.InvalidOperationException("Site variable is read-only");
            }
        }

        bool ISiteVar<bool>.ActiveSiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Site variable is read-only");
            }
        }

        bool ISiteVar<bool>.InactiveSiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Site variable is read-only");
            }
        }

        bool ISiteVar<bool>.SiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Site variable is read-only");
            }
        }
        #endregion
    }
}
