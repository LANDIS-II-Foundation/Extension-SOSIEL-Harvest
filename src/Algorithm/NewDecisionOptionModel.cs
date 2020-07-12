/// Name: NewDecisionOptionModel.cs
/// Description: The source code file used to create new decision options.
/// Authors: Multiple.
/// Last updated: July 10th, 2020.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

namespace Landis.Extension.SOSIELHarvest.Algorithm
{
    public class NewDecisionOptionModel
    {
        public string ManagementArea { get; set; }

        public string Name { get; set; }

        public string ConsequentVariable { get; set; }

        public double ConsequentValue { get; set; }

        public string BasedOn { get; set; }
    }
}
