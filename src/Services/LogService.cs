﻿// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.IO;

namespace Landis.Extension.SOSIELHarvest.Services
{
    public class LogService : IDisposable
    {
        private const string kLogFileName = "sosiel-harvest-log.txt";
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
