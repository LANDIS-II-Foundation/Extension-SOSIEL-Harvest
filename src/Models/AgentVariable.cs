namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentVariable: IVariable
    {
        public string Key => Agent;

        public string Agent { get; set; }

        public string VariableName { get; set; }

        public string VariableType { get; set; }

        public string VariableValue { get; set; }
    }
}