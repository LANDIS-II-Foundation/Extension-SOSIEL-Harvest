/// Name: ISimpleLogger.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

namespace Landis.Extension.SOSIELHarvest.SOSIELExtensions.Services
{
    /// <summary>
    /// Simple logging interface to be used by extension factory and extension itself.
    /// </summary>
    public interface ISimpleLogger
    {
        /// <summary>
        /// Writes message into log.
        /// </summary>
        /// <param name="message">A message to write.</param>
        void WriteLine(string message);

        /// <summary>
        /// Writes message to log with source method information.
        /// </summary>
        /// <param name="message">A message to write.</param>
        /// <param name="indent">An indent to put before the whole log output.</param>
        void Trace(string message, string indent = "");
    }
}
