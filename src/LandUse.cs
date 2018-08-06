// This file is part of the Social Human extension for LANDIS-II.

using Landis.SpatialModeling;

namespace Landis.Extension.SocialHuman
{
    /// <summary>
    /// The definition of a particular land use.
    /// </summary>
    public class LandUse
    {
        public string Name { get; protected set; }
        //public ushort MapCode { get; protected set; }
        public bool AllowHarvest { get; protected set; }
        public bool AllowEstablishment { get; protected set; }
        public LandCover.IChange LandCoverChange { get; protected set; }

        //---------------------------------------------------------------------

        public LandUse(
            string name,
            //ushort mapCode,
            bool harvestingAllowed,
            bool establishmentAllowed,
            LandCover.IChange initialLCC)
        {
            Name = name;
            //MapCode = mapCode;
            AllowHarvest = harvestingAllowed;
            AllowEstablishment = establishmentAllowed;
            LandCoverChange = initialLCC;
        }
    }
}
