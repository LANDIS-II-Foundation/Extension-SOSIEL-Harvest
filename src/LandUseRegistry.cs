// This file is part of the Social Human extension for LANDIS-II.

using System.Collections.Generic;

namespace Landis.Extension.SocialHuman
{
    public static class LandUseRegistry
    {
        private static Dictionary<ushort, LandUse> registry;

        //---------------------------------------------------------------------

        static LandUseRegistry()
        {
            registry = new Dictionary<ushort, LandUse>();
        }

        //---------------------------------------------------------------------

        public static void Register(LandUse landUse)
        {
            if (landUse == null)
                throw new System.ArgumentNullException();
            registry[landUse.MapCode] = landUse;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Look up a land use by its map code.
        /// </summary>
        public static LandUse LookUp(ushort mapCode)
        {
            LandUse landUse;
            if (registry.TryGetValue(mapCode, out landUse))
                return landUse;
            else
                return null;
        }
    }
}
