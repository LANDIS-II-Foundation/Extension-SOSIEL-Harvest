/// Name: VBGPConfigurationReader.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using CsvHelper;

using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Interfaces;
using Landis.Extension.SOSIELHarvest.SOSIELExtensions.Services;

using SOSIEL.VBGP.Configuration;

namespace Landis.Extension.SOSIELHarvest.SOSIELExtensions.VBGP
{
    /// <summary>
    /// Reads SOSIEL VBGP extension confguration from a CSV file.
    /// </summary>
    class VBGPConfigurationReader: IDisposable
    {
        private static readonly string kGainOrLossColumnName = "Gain/Loss";
        private static readonly string kValueColumnName = "Value";

        private readonly string _configFile;
        private ISimpleLogger _logger;

        private int _fieldCount;
        private CsvReader _csv;

        private class GoalColumns
        {
            public int GainOrLossColumnIndex = -1;
            public int ValueColumnIndex = -1;
            public List<int> AllColumnIndices = new List<int>();
        }

        /// <summary>
        /// Initializes new instance of the VBGPConfigurationReader class.
        /// </summary>
        /// <param name="parameters">Configuration parameters.</param>
        public VBGPConfigurationReader(IReadOnlyDictionary<string, string> parameters, ISimpleLogger logger)
        {
            _configFile = parameters[SOSIELExtensionParameterName.kConfig];
            _logger = logger;
        }
        public void Dispose()
        {
            _logger = null;
        }

        /// <summary>
        /// Reads configuration from file into the configuration object.
        /// </summary>
        /// <param name="config">Configuration file path.</param>
        /// <param name="logger">A logger.</param>
        /// <returns>Configuration object.</returns>
        public ValueBasedGoalPrioritizingConfiguration ReadConfiguration()
        {
            using (var reader = new StreamReader(_configFile))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                _logger.WriteLine($"    {VBGPExtensionId.kValue}: Reading configuration from the file '{_configFile}'");
                _csv = csv;
                try
                {
                    // Read first CSV header and get number of fields
                    _fieldCount = ReadNextLineAndCheckFieldCount();
                    if (_fieldCount < 1)
                        throw new InvalidDataException($"{_configFile}: Missing first CSV header");
                    if (_fieldCount % 2 == 1)
                    {
                        throw new InvalidDataException($"{_configFile}: There is odd number of fields, " +
                            "but it must be even");
                    }

                    // Parse goal names and collect number of columns per goal
                    var goalColumnMapping = new Dictionary<string, GoalColumns>();
                    for (int i = 0; i < _fieldCount; ++i)
                    {
                        var goalName = csv.GetField(i);
                        if (string.IsNullOrEmpty(goalName))
                            throw new InvalidDataException($"{_configFile}: Invalid goal name in the column #{i + 1}");

                        GoalColumns goalColumns = null;
                        if (!goalColumnMapping.TryGetValue(goalName, out goalColumns))
                        {
                            goalColumns = new GoalColumns();
                            goalColumnMapping.Add(goalName, goalColumns);
                        }

                        goalColumns.AllColumnIndices.Add(i);
                        
                        // Do not goal to have more than 2 columns
                        if (goalColumns.AllColumnIndices.Count > 2)
                            throw new InvalidDataException($"{_configFile}: Too many columns for the goal '{goalName}'");
                    }

                    // Do not allow goal to have less than 2 columns
                    var goalMissingColumn = goalColumnMapping.Where(kvp => kvp.Value.AllColumnIndices.Count < 2)
                            .Select(kvp => kvp.Key).FirstOrDefault();
                    if (goalMissingColumn != null)
                        throw new InvalidDataException($"{_configFile}: Too few columns for the goal '{goalMissingColumn}'");

                    // Read 2nd CSV header
                    if (ReadNextLineAndCheckFieldCount() < 0)
                        throw new InvalidDataException($"{_configFile}: Missing second CSV header");

                    // Find out which column corresponds to what
                    foreach (var kvp in goalColumnMapping)
                    {
                        foreach (int columnIndex in kvp.Value.AllColumnIndices)
                        {
                            var columnName = csv.GetField(columnIndex);
                            if (columnName == kGainOrLossColumnName)
                            {
                                if (kvp.Value.GainOrLossColumnIndex < 0)
                                    kvp.Value.GainOrLossColumnIndex = columnIndex;
                                else
                                    ReportDuplicateColumn(kvp.Key, columnIndex, columnName);
                            }
                            else if (columnName == kValueColumnName)
                            {
                                if (kvp.Value.ValueColumnIndex < 0)
                                    kvp.Value.ValueColumnIndex = columnIndex;
                                else
                                    ReportDuplicateColumn(kvp.Key, columnIndex, columnName);
                            }
                            else
                                ReportInvalidColumn(kvp.Key, columnIndex, columnName, "Unrecognized");
                        }
                    }

                    // Create data storage
                    List<GainOrLossToValueMappingElement>[] dataLists =
                            new List<GainOrLossToValueMappingElement>[goalColumnMapping.Count];
                    for (int i = 0; i < dataLists.Length; ++i)
                        dataLists[i] = new List<GainOrLossToValueMappingElement>();

                    // Parse data
                    while (ReadNextLineAndCheckFieldCount() > 0)
                    {
                        int i = 0;
                        foreach (var kvp in goalColumnMapping)
                        {
                            int currentColumn = -1;
                            GainOrLossToValueMappingElement e;
                            try
                            {
                                // Allow skipping a key/value if number of values differ for a different goals
                                currentColumn = kvp.Value.GainOrLossColumnIndex;
                                var argument = csv.GetField(currentColumn).Trim();
                                if (argument == "-")
                                {
                                    ++i;
                                    continue;
                                }
                                e.Argument = double.Parse(argument);

                                currentColumn = kvp.Value.ValueColumnIndex;
                                var value = csv.GetField(currentColumn).Trim();
                                e.Value = double.Parse(value);
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidDataException($"{_configFile}:{_csv.Context.RawRow}: " +
                                    $"Invalid floating point number in the column #{currentColumn + 1}: {ex.Message}");
                            }
                            dataLists[i++].Add(e);
                        }
                    }

                    // Create resulting configuration object
                    var dataDict = new Dictionary<string, GainOrLossToValueMappingElement[]>();
                    int ii = 0;
                    foreach (var kvp in goalColumnMapping)
                        dataDict.Add(kvp.Key, dataLists[ii++].ToArray());

                    return new ValueBasedGoalPrioritizingConfiguration(dataDict);
                }
                finally
                {
                    _csv = null;
                }
            }
        }

        private int ReadNextLineAndCheckFieldCount()
        {
            if (!_csv.Read()) return -1;

            int fieldCount = 0;
            while (_csv.TryGetField(fieldCount, out string _)) ++fieldCount;

            if (_fieldCount > 0 && fieldCount != _fieldCount)
            {
                throw new InvalidDataException($"{_configFile}:{_csv.Context.RawRow}: " +
                    $"Invalid number of fields, expecting {_fieldCount}, but received {fieldCount}");
            }

            return fieldCount;
        }

        private static void ReportDuplicateColumn(string goalName, int columnIndex, string columnName)
        {
            ReportInvalidColumn(goalName, columnIndex, columnName, "Duplicate");
        }

        private static void ReportInvalidColumn(string goalName, int columnIndex, string columnName,
                string errorType)
        {
            throw new InvalidDataException($"{errorType} column '{columnName}' " +
                $"in the column #{columnIndex + 1} for the goal '{goalName}'");
        }
    }
}
