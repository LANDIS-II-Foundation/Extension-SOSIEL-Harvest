// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;

using Landis.Extension.SOSIELHarvest.Models;
using Landis.Utilities;

namespace Landis.Extension.SOSIELHarvest.Input
{
    public class SheParameters
    {
        public const int MinModeNumber = 1;
        public const int MaxModeNumber = 3;
        public const int NumberOfModes = MaxModeNumber - MinModeNumber + 1;

        private List<int> _modes;
        private IReadOnlyList<int> _roModes;

        private int _timestep;

        public IReadOnlyList<int> Modes
        {
            get => _roModes;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value), "Mode list is null");
                if (value.Count == 0) throw new InputValueException(value.ToString(), "Mode list is empty");
                if (value.Count > NumberOfModes)
                {
                    throw new InputValueException(
                        value.ToString(), $"Mode list contains more than {NumberOfModes} modes");
                }

                int foundModes = 0;
                foreach (int mode in value)
                {
                    if (!ValidateModeNumber(mode))
                    {
                        throw new InputValueException(value.ToString(),
                            $"Mode number is not in the range from {MinModeNumber} to {MaxModeNumber} inclusive");
                    }
                    var modeBit = 1 << (mode - MinModeNumber);
                    if ((foundModes & modeBit) != 0)
                        throw new InputValueException(value.ToString(), $"Duplicate mode number {mode}");
                    foundModes |= modeBit;
                }

                var modes = new List<int>();
                modes.AddRange(value);
                var roModes = modes.AsReadOnly();
                _modes = modes;
                _roModes = roModes;
            }
        }

        public bool IsMultiMode
        {
            get => _modes.Count > 1;
        }

        public int Timestep
        {
            get => _timestep;
            set
            {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Timestep must be > or = 0");
                _timestep = value;
            }
        }

        public string SosielInitializationFileName { get; set; }

        public string BiomassHarvestInitializationFileName { get; set; }
        
        public string ManagementAreaFileName { get; set; }

        public string StandsFileName { get; set; }

        public List<AgentToManagementArea> AgentToManagementAreaList { get; set; }

        public List<Prescription> Prescriptions { get; set; }
        
        public string PrescriptionMapsOutput { get; set; }
        
        public string EventLogOutput { get; set; }
        
        public string SummaryOutput { get; set; }

        public SheParameters()
        {
            _modes = new List<int>();
            _modes.Add(2);
            _roModes = _modes.AsReadOnly();
        }

        public static bool ValidateModeNumber(int mode)
        {
            return mode >= MinModeNumber && mode <= MaxModeNumber;
        }
    }
}
