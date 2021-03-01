/// Name: VBGPExtensionFactory.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;

using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Attributes;
using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Interfaces;
using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Services;

namespace Landis.Extension.SOSIELHarvest.SOSIELExtensions.VBGP
{
    class VBGPExtensionId
    {
        public static readonly string kValue = "SOSIEL.VBGP";
    }

    /// <summary>
    /// Implements extension factory for the SOSIEL VBGP extension.
    /// </summary>
    [SOSIELExtensionFactory("SOSIEL.VBGP")]
    public class VBGPExtensionFactory : ISOSIELExtensionFactory
    {
        private ISimpleLogger _logger;

        /// <summary>
        /// Initializes new instance on the VBGPExtensionFactory class.
        /// </summary>
        /// <param name="log">Logger object.</param>
        public VBGPExtensionFactory(ISimpleLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates SOSIEL extension instance.
        /// </summary>
        /// <param name="parameters">Create extension parameters</param>
        /// <returns>Extension object or null if extension is not supported.</returns>
        public ISOSIELExtension CreateInstance(IReadOnlyDictionary<string, string> parameters)
        {
            var extensionId = parameters[SOSIELExtensionParameterName.kExtensionId];
            if (extensionId != VBGPExtensionId.kValue) return null;
            return new VBGPExtension(parameters, _logger);
        }

        public void Dispose()
        {
            _logger = null;
        }
    }
}
