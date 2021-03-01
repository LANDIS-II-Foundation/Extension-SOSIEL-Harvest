/// Name: MapPixel.cs
/// Description: 
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHarvest
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
