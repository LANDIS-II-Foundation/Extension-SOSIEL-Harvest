using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Environments;

    public sealed class RuleConsequent : ICloneable<RuleConsequent>, IEquatable<RuleConsequent>
    {
        public string Param { get; private set; }

        public dynamic Value { get; private set; }

        public string VariableValue { get; private set; }

        public bool CopyToCommon { get; private set; }

        public bool SavePrevious { get; private set; }

        public RuleConsequent(string param, dynamic value, string variableValue = null, bool copyToCommon = false, bool savePrevious = false)
        {
            Param = param;
            Value = value;
            VariableValue = variableValue;
            CopyToCommon = copyToCommon;
            SavePrevious = savePrevious;
        }


        /// <summary>
        /// Creates shallow object copy 
        /// </summary>
        /// <returns></returns>
        public RuleConsequent Clone()
        {
            return (RuleConsequent)MemberwiseClone();
        }

        /// <summary>
        /// Creates copy of consequent but replaces consequent constant by new constant value. 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static RuleConsequent Renew(RuleConsequent old, dynamic newValue)
        {
            RuleConsequent newConsequent = old.Clone();

            newConsequent.Value = newValue;

            return newConsequent;
        }


        /// <summary>
        /// Compares two RuleConsequent objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(RuleConsequent other)
        {
            //check on reference equality first
            //custom logic for comparing two objects
            return ReferenceEquals(this, other) 
                || (other != null && Param == other.Param && Value == other.Value && VariableValue == other.VariableValue);
        }

        public override bool Equals(object obj)
        {
            //check on reference equality first
            return base.Equals(obj) || Equals(obj as RuleConsequent);
        }

        public override int GetHashCode()
        {
            //disable comparing by hash code
            return 0;
        }

        public static bool operator ==(RuleConsequent a, RuleConsequent b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(RuleConsequent a, RuleConsequent b)
        {
            return !(a == b);
        }
    }
}
