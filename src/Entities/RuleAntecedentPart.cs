using System;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.SOSIELHuman.Entities
{
    using Environments;
    using Helpers;

    public class RuleAntecedentPart : ICloneable<RuleAntecedentPart>, IEquatable<RuleAntecedentPart>
    {


        private Func<dynamic, dynamic, dynamic> antecedent;

        public string Param { get; private set; }

        public string Sign { get; private set; }

        public dynamic Value { get; private set; }

        public string ReferenceVariable { get; private set; }


        public RuleAntecedentPart(string param, string sign, dynamic value, string referenceVariable = null)
        {
            Param = param;
            Sign = sign;
            Value = value;
            ReferenceVariable = referenceVariable;
        }

        /// <summary>
        /// Creates expression tree for condition checking
        /// </summary>
        private void BuildAntecedent()
        {
            antecedent = AntecedentBuilder.Build(Sign);
        }

        /// <summary>
        /// Checks agent variables on antecedent part condition
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        public bool IsMatch(IAgent agent)
        {
            if (antecedent == null)
            {
                BuildAntecedent();
            }

            dynamic value = Value;

            if (string.IsNullOrEmpty(ReferenceVariable) == false)
            {
                value = agent[ReferenceVariable];
            }

            return antecedent(agent[Param], value);
        }

        /// <summary>
        /// Creates shallow object copy 
        /// </summary>
        /// <returns></returns>
        public RuleAntecedentPart Clone()
        {
            return (RuleAntecedentPart)MemberwiseClone();
        }

        /// <summary>
        /// Creates copy of antecedent part but replaces antecedent constant by new constant value. 
        /// </summary>
        /// <param name="old"></param>
        /// <param name="newConst"></param>
        /// <returns></returns>
        public static RuleAntecedentPart Renew(RuleAntecedentPart old, dynamic newConst)
        {
            RuleAntecedentPart newAntecedent = old.Clone();

            newAntecedent.antecedent = null;

            newAntecedent.Value = newConst;

            return newAntecedent;
        }

        /// <summary>
        /// Compares two RuleAntecedentPart objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(RuleAntecedentPart other)
        {
            //check on reference equality first
            //custom logic for comparing two objects
            return ReferenceEquals(this, other) 
                || (other != null && Param == other.Param && Sign == other.Sign && Value == other.Value && ReferenceVariable == other.ReferenceVariable);
        }

        public override bool Equals(object obj)
        {
            //check on reference equality first
            return base.Equals(obj) || Equals(obj as RuleAntecedentPart);
        }

        public override int GetHashCode()
        {
            //disable comparing by hash code
            return 0;
        }

        public static bool operator ==(RuleAntecedentPart a, RuleAntecedentPart b)
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

        public static bool operator !=(RuleAntecedentPart a, RuleAntecedentPart b)
        {
            return !(a == b);
        }
    }
}
