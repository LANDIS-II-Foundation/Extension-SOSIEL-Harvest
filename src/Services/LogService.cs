// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

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
            PlugIn.ModelCore.UI.WriteLine(logEntry);
            _streamWriter.WriteLine(logEntry);
            _streamWriter.Flush();
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
        }
    }
}
