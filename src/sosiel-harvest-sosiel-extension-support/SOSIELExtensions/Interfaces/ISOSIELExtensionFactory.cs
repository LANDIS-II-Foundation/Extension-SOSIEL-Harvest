/// Name: ISOSIELExtensionFactory.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Collections.Generic;

namespace Landis.Extension.SOSIELHarvest.SOSIELExtensions.Interfaces
{
    /// <summary>
    /// Provides extension information and creates SOSIEL extension instances.
    /// </summary>
    public interface ISOSIELExtensionFactory : IDisposable
    {
        /// <summary>
        /// Creates SOSIEL extension instance.
        /// </summary>
        /// <param name="parameters">Create extension parameters</param>
        /// <returns>Extension object or null if extension is not supported.</returns>
        ISOSIELExtension CreateInstance(IReadOnlyDictionary<string, string> parameters);
    }
}
