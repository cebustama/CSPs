using System;
using System.Collections.Generic;
using System.Linq;

public partial class COP<T>
{
    #region Constraint Types
    /// <summary>
    /// Single Constraint for this COP
    /// Can include many variables
    /// </summary>
    /// <typeparam name="C">Type of variables being compared in condition</typeparam>
    [Serializable]
    public class Constraint<C>
    {
        public int[] variableIDs;

        public Func<C[], bool> condition;

        public Constraint(string[] variableIDs, Func<C[], bool> condition)
        {
            this.variableIDs = Array.ConvertAll(variableIDs, s => s.GetHashCode());
            this.condition = condition;
        }

        public bool Check(C[] values)
        {
            return condition(values);
        }
    }

    [Serializable]
    public class HardConstraint<C> : Constraint<C>
    {
        public HardConstraint(string[] variableIDs, Func<C[], bool> condition) : base(variableIDs, condition)
        {
        }
    }

    /// <summary>
    /// Constraint between two variables
    /// </summary>
    /// <typeparam name="C"></typeparam>
    public class BinaryConstraint<C> : Constraint<C>
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

    #endregion

    // TODO: IMPLEMENT ADDING HARD CONSTRAINT
    // BY SETTING THE UTILITY FUNCTION OF INVOLVED VARIABLES
    protected virtual void AddConstraint(string[] variables, Func<T[], bool> condition)
    {
        Constraint<T> constraint = new Constraint<T>(variables, condition);
        Constraints.Add(constraint);
        ProcessConstraint(variables, constraint);
    }

    protected virtual void ProcessConstraint(string[] vIds, Constraint<T> constraint)
    {
        // Store constraints associated with each variable
        foreach (string v in vIds)
        {
            int id = v.GetHashCode();
            if (!ConstraintsDictionary.ContainsKey(id))
                ConstraintsDictionary.Add(id, new List<Constraint<T>>());

            ConstraintsDictionary[id].Add(constraint);
        }
    }

    public List<Constraint<T>> GetConstraintsFromTo(string v1, string v2)
    {
        List<Constraint<T>> constraints = new List<Constraint<T>>();

        int v1Id = v1.GetHashCode();
        int v2Id = v2.GetHashCode();

        // One way
        foreach (Constraint<T> c in ConstraintsDictionary[v1Id])
        {
            if (c.variableIDs.Contains(v2Id)) constraints.Add(c);
        }

        return constraints;
    }
}