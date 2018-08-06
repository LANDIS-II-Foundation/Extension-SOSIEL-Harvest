// This file is part of the Social Human extension for LANDIS-II.

using Landis.SpatialModeling;

namespace Landis.Extension.SocialHuman
{
    public class MapPixel : Pixel
    {
        public Band<ushort> LandUseCode  = "Map code for a site's land use";

        public MapPixel()
        {
            SetBands(LandUseCode);
        }
    }
}
