// Copyright (C) 2018-2021 The SOSIEL Foundation. All rights reserved.
// Use of this source code is governed by a license that can be found
// in the LICENSE file located in the repository root directory.

using System;
using System.Collections.Generic;
using System.Linq;

using Landis.Library.HarvestManagement;
using Landis.SpatialModeling;

using SOSIEL.Entities;

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class Area : IDataSet
    {
        private readonly List<ActiveSite> _sites;

        public Area()
        {
            AssignedAgents = new List<string>();
            _sites = new List<ActiveSite>();
        }

        public string Name { get; private set; }

        public List<string> AssignedAgents { get; set; }

        public ManagementArea ManagementArea { get; private set; }

        public void Initialize(ManagementArea managementArea)
        {
            Name = managementArea.MapCode.ToString();
            ManagementArea = managementArea;
            foreach (var stand in ManagementArea)
            foreach (var activeSite in stand)
                _sites.Add(activeSite);
        }

        public ActiveSite GetRandomSite()
        {
            var count = _sites.Count;
            var random = new Random();
            var index = random.Next(0, count - 1);
            return _sites[index];
        }

        public Dictionary<Location, ActiveSite> GetActiveSites()
        {
            return _sites.ToDictionary(site => site.Location, site => site);
        }
    }
}
