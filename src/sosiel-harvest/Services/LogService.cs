/// Name: LogService.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Diagnostics;
using System.IO;

using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Services;

namespace Landis.Extension.SOSIELHarvest.Services
{
    public class LogService : IDisposable, ISimpleLogger
    {
        private static readonly string kLogFileName = "sosiel-harvest-log.txt";
 
        private StreamWriter _streamWriter;

        public void StartService()
        {
            _streamWriter = File.CreateText(kLogFileName);
        }

        public void StopService()
        {
            _streamWriter.Flush();
            _streamWriter.Close();
        }

        public void WriteLine(string message)
        {
            PlugIn.Core.UI.WriteLine(logEntry);
            _streamWriter.WriteLine(message);
            _streamWriter.Flush();
        }

        public void Trace(string message, string indent = "")
        {
            var method = (new StackFrame(1)).GetMethod();
            WriteLine($"{indent}*** TRACE ***: {method.DeclaringType.FullName}.{method.Name}: {message}");
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
        }
    }
}