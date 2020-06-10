namespace Landis.Extension.SOSIELHarvest.Models
{
    public interface IVariable
    {
        string Key { get; }

        string VariableName { get; }

        string VariableType { get; }

        string VariableValue { get;  }
    }
}