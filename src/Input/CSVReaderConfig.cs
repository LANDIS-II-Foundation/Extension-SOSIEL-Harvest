// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System.Globalization;

namespace Landis.Extension.SOSIELHarvest.Input
{
    class CSVReaderConfig
    {
        public static readonly CsvHelper.Configuration.Configuration config =
            new CsvHelper.Configuration.Configuration(CultureInfo.InvariantCulture);
    }
}
