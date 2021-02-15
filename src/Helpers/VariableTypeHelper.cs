/// Name: VariableTypeHelper.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;

namespace Landis.Extension.SOSIELHarvest.Helpers
{
    public static class VariableTypeHelper
    {
        /// <summary>
        /// Converts type name to System.Type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Unknown variable type:" + type</exception>
        public static Type ConvertStringToType(string type)
        {
            var lowerCase = type.ToLowerInvariant();

            switch (lowerCase)
            {
                case "integer": return typeof(int);
                case "number": return typeof(double);
                case "string": return typeof(string);
                default:
                    throw new ArgumentOutOfRangeException("Unknown variable type:" + type);
            }
        }

    }
}
