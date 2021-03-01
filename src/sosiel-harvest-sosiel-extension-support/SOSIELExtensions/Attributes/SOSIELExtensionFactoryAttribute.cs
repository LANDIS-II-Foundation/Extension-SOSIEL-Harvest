/// Name: SOSIELExtensionFactoryAttribute.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Collections.Generic;

namespace Landis.Extension.SOSIELHarvest.SOSIELExtensions.Attributes
{
    /// <summary>
    /// Marks class as SOSIEL extension factory for the given extension ID.
    /// The class must also support interface ISOSIELExtensionFactory to qualify such.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class SOSIELExtensionFactoryAttribute : Attribute
    {
        /// <summary>
        /// SOSIEL extension identifier.
        /// </summary>
        public string ExtensionId { get;  }

        /// <summary>
        /// Initializes new instance of the SOSIELExtensionFactoryAttribute class.
        /// </summary>
        /// <param name="extensionId">Unique SOSIEL extension identifier.</param>
        public SOSIELExtensionFactoryAttribute(string extensionId)
        {
            ExtensionId = extensionId;
        }
    }
}
