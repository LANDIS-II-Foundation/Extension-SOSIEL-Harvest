/// Name: VBGPExtension.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System.Collections.Generic;

using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Interfaces;
using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Services;

using SOSIEL.VBGP.Processes;

namespace Landis.Extension.SOSIELHarvest.SOSIELExtensions.VBGP
{
    /// <summary>
    /// Implement SOSIEL extension interface for the SOSIEL VBGP extension.
    /// </summary>
    class VBGPExtension : ISOSIELExtension
    {
        private readonly IReadOnlyDictionary<string, string> _parameters;
        private ValueBasedGoalPrioritizing _object;

        /// <summary>
        /// Extension category.
        /// </summary>
        public string ExtensionCategory => SOSIELExtensionPropertyValue.kGoalPrioritizingExtensionCategory;

        /// <summary>
        /// Extension configuration.
        /// </summary>
        public IReadOnlyDictionary<string, string> ExtensionParameters => _parameters;

        /// <summary>
        /// Extension specific object.
        /// </summary>
        public object ExtensionObject => _object;

        /// <summary>
        /// Initializes new instance of the VBGPExtension class.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        /// <param name="logger">A logger.</param>
        public VBGPExtension(IReadOnlyDictionary<string, string> parameters, ISimpleLogger logger)
        {
            _parameters = parameters;
            using (var configReader = new VBGPConfigurationReader(parameters, logger))
            {
                var configuration = configReader.ReadConfiguration();
                logger.WriteLine($"    {VBGPExtensionId.kValue}: Configuration details:");
                logger.WriteLine($"    {VBGPExtensionId.kValue}: Number of goals: {configuration.GoalCount}");

                string dumpGainsAndLossesValue;
                parameters.TryGetValue("dumpGainsAndLosses", out dumpGainsAndLossesValue);
                var dumpGainsAndLossesFlag = !string.IsNullOrEmpty(dumpGainsAndLossesValue)
                    && dumpGainsAndLossesValue == "true";

                foreach (var kvp in configuration.GainAndLossToValue)
                {
                    logger.WriteLine($"    {VBGPExtensionId.kValue}: Goal '{kvp.Key}': " +
                        $"gains: {kvp.Value.GainToValue.Length}, losses: {kvp.Value.LossToValue.Length}, " +
                        $"total: {kvp.Value.GainToValue.Length + kvp.Value.LossToValue.Length}");

                    if (dumpGainsAndLossesFlag)
                    {
                        logger.WriteLine($"    {VBGPExtensionId.kValue}: Gains:");
                        int index = 0;
                        foreach (var e in kvp.Value.GainToValue)
                            logger.WriteLine($"    {VBGPExtensionId.kValue}: [{index++}] {e.Argument} => {e.Value}");

                        logger.WriteLine($"    {VBGPExtensionId.kValue}: Losses:");
                        index = 0;
                        foreach (var e in kvp.Value.LossToValue)
                            logger.WriteLine($"    {VBGPExtensionId.kValue}: [{index++}] {e.Argument} => {e.Value}");
                    }
                }
                _object = new ValueBasedGoalPrioritizing(configuration);
            }
        }

        /// <summary>
        /// De-initialize extension and free resources.
        /// After calling this method, extension instance is no longer usable.
        /// </summary>
        public void Deinitialize()
        {
            _object = null;
        }
    }
}
