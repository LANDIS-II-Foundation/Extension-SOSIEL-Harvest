/// Name: IVariable.cs
/// Description:
/// Authors: Multiple.
/// Copyright: Garry Sotnik, Brooke A. Cassell, Robert M. Scheller.

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