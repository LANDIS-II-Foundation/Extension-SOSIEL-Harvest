// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (C) 2021 SOSIEL Inc. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Landis.Extension.SOSIELHarvest.Algorithm;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.SOSIELHarvest.Models
{
    public class HarvestManager
    {
        private Random _random;
        private readonly IEnumerable<Prescription> _prescriptions;
        private readonly ISiteVar<string> _harvestPrescriptionName;
        private readonly ISiteVar<SiteCohorts> _siteCohorts;

        private bool _isHarvestingFinished;
        private readonly Dictionary<Location, ActiveSite> _availableSites;
        private readonly HashSet<Location> _harvestedSites;

        private int _maxColumn;
        private int _maxRow;

        public int HarvestedSitesNumber => _harvestedSites.Count;

        public HarvestManager(Area area, IEnumerable<Prescription> prescriptions,
            ISiteVar<string> harvestPrescriptionName, ISiteVar<SiteCohorts> siteCohorts)
        {
            _prescriptions = prescriptions;
            _harvestPrescriptionName = harvestPrescriptionName;
            _siteCohorts = siteCohorts;
            _random = new Random();
            _availableSites = area.GetActiveSites();
            _maxColumn = _availableSites.Keys.Max(location => location.Column);
            _maxRow = _availableSites.Keys.Max(location => location.Row);
            _harvestedSites = new HashSet<Location>();
        }

        public int Harvest()
        {
            if (_isHarvestingFinished)
                throw new InvalidOperationException();

            if (!_prescriptions.Any())
                return 0;

            var harvested = new Dictionary<Prescription, int>();

            foreach (var prescription in _prescriptions)
                harvested[prescription] = 0;

            foreach (var availableSite in GetSitesByRandomWalk())
            {
                foreach (var prescription in _prescriptions)
                {
                    var siteCohorts = _siteCohorts[availableSite];
                    if (prescription.CheckSiteToHarvest(siteCohorts))
                    {
                        if (harvested.ContainsKey(prescription) &&
                            harvested[prescription] >= prescription.TargetHarvestSize)
                            continue;

                        _harvestPrescriptionName[availableSite] = prescription.Name;
                        harvested[prescription] += HarvestSite(prescription, availableSite, siteCohorts);
                    }
                }

                _harvestedSites.Add(availableSite.Location);

                if (CheckIsLimitReached(harvested))
                    break;
            }

            _isHarvestingFinished = true;
            return harvested.Sum(pair => pair.Value);
        }

        private int HarvestSite(Prescription prescription, ActiveSite activeSite, ISiteCohorts siteCohorts)
        {
            int harvested = 0;
            foreach (var siteHarvestingRule in prescription.SiteHarvestingRules)
                harvested += siteCohorts.ReduceOrKillCohorts(new Disturbance(activeSite, siteHarvestingRule));
            //SiteVars.Cohorts[site].ReduceOrKillCohorts(this);
            return harvested;
        }

        private bool CheckIsLimitReached(Dictionary<Prescription, int> harvested)
        {
            foreach (var prescription in _prescriptions)
            {
                if (harvested.ContainsKey(prescription) &&
                    harvested[prescription] < prescription.TargetHarvestSize)
                    return false;
            }

            return true;
        }

        private IEnumerable<ActiveSite> GetSitesByRandomWalk()
        {
            bool isSiteExist = false;
            var firstSite = new ActiveSite();
            do
            {
                var row = _random.Next(_maxRow);
                var column = _random.Next(_maxColumn);
                var location = new Location(row, column);
                if (_availableSites.ContainsKey(location))
                {
                    firstSite = _availableSites[location];
                    yield return firstSite;
                    isSiteExist = true;
                }
            } while (isSiteExist == false);

            List<Location> neighboursCoordinates;
            do
            {
                neighboursCoordinates = GetAvailableNeighborCoordinates(firstSite);
                if (neighboursCoordinates.Count == 0)
                    yield break;

                var location = neighboursCoordinates[_random.Next(neighboursCoordinates.Count - 1)];
                yield return _availableSites[location];
            } while (neighboursCoordinates.Count != 0);
        }

        private List<Location> GetAvailableNeighborCoordinates(ActiveSite activeSite)
        {
            var availableSitesCoordinates = new List<Location>();
            var location = activeSite.Location;
            int distance = 1;

            while (availableSitesCoordinates.Count == 0 && (activeSite.Location.Row + distance <= _maxRow ||
                                                            activeSite.Location.Row - distance >= 0
                                                            || activeSite.Location.Column + distance <=
                                                            _maxColumn &&
                                                            activeSite.Location.Column >= 0))
            {
                int row = location.Row - distance;
                row = NormalizeRow(row);
                int column = location.Column - distance;
                column = NormalizeColumn(column);
                var leftTopCorner = new Location(row, column);
                row = location.Row - distance;
                row = NormalizeRow(row);
                column = location.Column + distance;
                column = NormalizeColumn(column);
                var rightTopCorner = new Location(row, column);
                row = location.Row + distance;
                row = NormalizeRow(row);
                column = location.Column + distance;
                column = NormalizeColumn(column);
                var rightBottomCorner = new Location(row, column);
                row = location.Row + distance;
                row = NormalizeRow(row);
                column = location.Column - distance;
                column = NormalizeColumn(column);
                var leftBottomCorner = new Location(row, column);

                var nextLocation = leftTopCorner;
                do
                {
                    availableSitesCoordinates.Add(nextLocation);
                    nextLocation = new Location(nextLocation.Row, nextLocation.Column + 1);
                } while (nextLocation != rightTopCorner);

                do
                {
                    availableSitesCoordinates.Add(nextLocation);
                    nextLocation = new Location(nextLocation.Row + 1, nextLocation.Column);
                } while (nextLocation != rightBottomCorner);

                do
                {
                    availableSitesCoordinates.Add(nextLocation);
                    nextLocation = new Location(nextLocation.Row, nextLocation.Column - 1);
                } while (nextLocation != leftBottomCorner);

                do
                {
                    availableSitesCoordinates.Add(nextLocation);
                    nextLocation = new Location(nextLocation.Row - 1, nextLocation.Column);
                } while (nextLocation != leftTopCorner);

                var toDelete = new List<Location>();
                foreach (var availableSitesCoordinate in availableSitesCoordinates)
                {
                    if (_harvestedSites.Contains(availableSitesCoordinate) || !_availableSites.ContainsKey(availableSitesCoordinate))
                        toDelete.Add(availableSitesCoordinate);
                }

                foreach (var locationToDelete in toDelete)
                {
                    availableSitesCoordinates.Remove(locationToDelete);
                    _harvestedSites.Add(locationToDelete);
                }

                distance++;
            }

            return availableSitesCoordinates;
        }

        private int NormalizeColumn(int column)
        {
            if (column < 0)
                return 0;
            if (column > _maxColumn)
                return _maxColumn;
            return column;
        }

        private int NormalizeRow(int row)
        {
            if (row < 0)
                return 0;
            if (row > _maxRow)
                return _maxRow;
            return row;
        }
    }
}
