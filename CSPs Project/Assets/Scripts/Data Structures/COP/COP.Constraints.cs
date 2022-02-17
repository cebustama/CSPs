using System;
using System.Collections.Generic;

public partial class COP<T>
{
    /// <summary>
    /// Single Constraint for this CSP
    /// Can include many variables
    /// </summary>
    /// <typeparam name="C">Type of variables being compared in condition</typeparam>
    [Serializable]
    public class COPConstraint<C>
    {
        public string[] variableIDs;

        public Func<C[], bool> condition;

        public COPConstraint(string[] variableIDs, Func<C[], bool> condition)
        {
            this.variableIDs = variableIDs;
            this.condition = condition;
        }

        public bool Check(C[] values)
        {
            return condition(values);
        }
    }

    /// <summary>
    /// Constraint between two variables
    /// </summary>
    /// <typeparam name="C"></typeparam>
    public class BinaryConstraint<C> : COPConstraint<C>
    {
        public string v1 { get; private set; }
        public string v2 { get; private set; }

        public BinaryConstraint(string[] variableIDs, Func<C[], bool> condition) : base(variableIDs, condition)
        {
            if (variableIDs.Length != 2)
                throw new ArgumentException();

            v1 = variableIDs[0];
            v2 = variableIDs[1];
        }
    }
}