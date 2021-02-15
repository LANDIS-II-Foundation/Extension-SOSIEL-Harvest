/// Name: LogService.cs
/// Description: 
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.IO;

namespace Landis.Extension.SOSIELHarvest.Services
{
    public class LogService : IDisposable
    {
        private const string LogFileName = "sosiel-harvest-log.txt";
        private StreamWriter _streamWriter;

        public void StartService()
        {
            _streamWriter = File.CreateText(LogFileName);
        }

        public void StopService()
        {
            _streamWriter.Flush();
            _streamWriter.Close();
        }

        public void WriteLine(string logEntry)
        {
            PlugIn.Core.UI.WriteLine(logEntry);
            _streamWriter.WriteLine(logEntry);
            _streamWriter.Flush();
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
        }
    }
}