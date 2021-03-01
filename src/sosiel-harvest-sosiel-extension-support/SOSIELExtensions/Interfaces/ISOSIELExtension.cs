/// Name: ISOSIELExtension.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;

namespace Landis.Extension.SOSIELHarvest.SOSIELExtensions.Interfaces
{
    /// <summary>
    /// SOSIEL extension interface.
    /// </summary>
    public interface ISOSIELExtension
    {
        /// <summary>
        /// Extension category.
        /// </summary>
        string ExtensionCategory { get; }

        /// <summary>
        /// Extension configuration parameters.
        /// </summary>
        IReadOnlyDictionary<string, string> ExtensionParameters { get; }

        /// <summary>
        /// Extension specific object.
        /// </summary>
        object ExtensionObject { get; }

        /// <summary>
        /// De-initialize extension and free resources.
        /// After calling this method, extension instance is no longer usable.
        /// </summary>
        void Deinitialize();
    }
}
