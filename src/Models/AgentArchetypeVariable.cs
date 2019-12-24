namespace Landis.Extension.SOSIELHarvest.Models
{
    public class AgentArchetypeVariable: IVariable
    {
        public string Key => ArchetypeName;

        public string ArchetypeName { get; set; }

        public string VariableName { get; set; }

        public string VariableType { get; set; }

        public string VariableValue { get; set; }
    }
}
