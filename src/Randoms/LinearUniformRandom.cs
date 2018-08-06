using System;

namespace Landis.Extension.SOSIELHuman.Randoms
{
    /// <summary>
    /// Implementation of linear uniform distribution random. Used standard .NET random.
    /// </summary>
    public sealed class LinearUniformRandom
    {
        private static Random random = new Random();

        public static Random GetInstance
        {
            get
            {
                return random;
            }
        }

        private LinearUniformRandom() { }
    }
}
