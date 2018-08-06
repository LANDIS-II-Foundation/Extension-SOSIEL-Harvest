// This file is part of the Social Human extension for LANDIS-II.

using Edu.Wisc.Forest.Flel.Util;

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
