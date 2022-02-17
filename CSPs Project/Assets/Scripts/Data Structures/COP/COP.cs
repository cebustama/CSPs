
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[Serializable]
/// <summary>
/// Constraint Optimization Problem
/// </summary>
/// <typeparam name="T">Datatype of value (variable of concrete domain)</typeparam>
public partial class COP<T>
{
    protected static System.Random rng = new System.Random();

    /// <summary>
    /// Single instance of a variable for this COP
    /// </summary>
    /// <typeparam name="V">Datatype of value, same as COP</typeparam>
    [Serializable]
    public class COPVariable<V>
    {
        public string name; // Unique Identifier
        public List<V> domain; // Use list in case of variable domains
        public V value;

        public void AssignRandom(System.Random rng)
        {
            if (domain.Count < 1)
                throw new ArgumentOutOfRangeException();

            value = domain[rng.Next(0, domain.Count)];
        }

        public bool AssignValue(V value, bool checkDomain = false)
        {
            if (!checkDomain)
                this.value = value;
            // Check domain
            else if (!domain.Contains(value))
                return false;

            return true;
        }

        public bool ValueInDomain()
        {
            return domain.Contains(value);
        }
    }

    public COPVariable<T>[] Variables { get; private set; }
    // TODO: Store index instead of variable
    public Dictionary<string, COPVariable<T>> VariablesDictionary { get; private set; }

    public List<COPConstraint<T>> Constraints { get; private set; }
    // TODO: Store index instead of constraint
    public Dictionary<string, List<COPConstraint<T>>> ConstraintsDictionary { get; private set; }

    public uint NumColors { get; private set; }

    public COP(string[] names, T[][] domains, T defaultValue = default(T))
    {
        Variables = new COPVariable<T>[names.Length];
        VariablesDictionary = new Dictionary<string, COPVariable<T>>();
        for (int i = 0; i < Variables.Length; i++)
        {
            Variables[i] = new COPVariable<T>()
            {
                name = names[i],
                domain = domains[i].ToList(),
                value = defaultValue
            };

            VariablesDictionary.Add(Variables[i].name, Variables[i]);
        }

        Constraints = new List<COPConstraint<T>>();
        ConstraintsDictionary = new Dictionary<string, List<COPConstraint<T>>>();

        NumColors = 0;
    }

    public void SetNumColors(uint num)
    {
        if (num == 0) throw new ArgumentOutOfRangeException();

        NumColors = num;
    }

    protected virtual void AddConstraint(string[] variables, Func<T[], bool> condition)
    {
        COPConstraint<T> constraint = new COPConstraint<T>(variables, condition);
        Constraints.Add(constraint);
        IndexConstraint(variables, constraint);
    }

    protected virtual void IndexConstraint(string[] variables, COPConstraint<T> constraint)
    {
        // Store constraints associated with each variable
        foreach (string v in variables)
        {
            if (!ConstraintsDictionary.ContainsKey(v))
                ConstraintsDictionary.Add(v, new List<COPConstraint<T>>());

            ConstraintsDictionary[v].Add(constraint);
        }
    }

    public List<COPConstraint<T>> GetConstraintsFromTo(string v1, string v2)
    {
        List<COPConstraint<T>> constraints = new List<COPConstraint<T>>();

        // One way
        foreach (COPConstraint<T> c in ConstraintsDictionary[v1])
        {
            if (c.variableIDs.Contains(v2)) constraints.Add(c);
        }

        return constraints;
    }

    public virtual void Solve() { }

    public virtual void SolveWithAgents(GameObject[] agents, string seed) { }

    public virtual void Step() { }

    public virtual List<COPVariable<T>> OrderVariables(Func<List<COPVariable<T>>, List<COPVariable<T>>> orderingMethod)
    {
        return orderingMethod(Variables.ToList());
    }

    public virtual bool AssignFirstValid(string varID, bool addValuesToDomain = false, Func<T> randomFunction = null)
    {
        foreach (T value in VariablesDictionary[varID].domain)
        {
            // Found valid value in domain
            if (IsConsistent(varID, value))
            {
                VariablesDictionary[varID].AssignValue(value);
                return true;
            }
        }

        // if no values in domain were cosistent, add new one
        if (addValuesToDomain)
        {
            if (randomFunction == null)
                throw new ArgumentException();

            VariablesDictionary[varID].AssignValue(randomFunction());
        }

        return false;
    }

    public virtual bool AssignValidOtherThan(string varID, T exception)
    {
        List<T> randomizedDomain = VariablesDictionary[varID].domain.OrderBy(a => rng.Next()).ToList();

        foreach (T value in randomizedDomain)
        {
            // Found valid value in domain
            if (!value.Equals(exception) && IsConsistent(varID, value))
            {
                VariablesDictionary[varID].AssignValue(value);
                return true;
            }
        }

        return false;
    }

    public virtual void AssignRandom(string varID, System.Random rng)
    {
        VariablesDictionary[varID].AssignRandom(rng);
    }

    public virtual void AssignValue(string varID, T value)
    {
        VariablesDictionary[varID].AssignValue(value);
    }

    // Checks solution for consistency
    public virtual bool IsConsistent(string varID, T value)
    {
        // Variable has no constraints
        if (!ConstraintsDictionary.ContainsKey(varID)) return true;

        //Debug.Log("Checking " + varID + " with value " + value.ToString() + " for " + ConstraintsDictionary[varID].Count + " constraints.");
        // Check all constraints that involve variable
        foreach (COPConstraint<T> c in ConstraintsDictionary[varID])
        {
            T[] values = new T[c.variableIDs.Length];
            // Get involved variables and store involved values, except the one being tested
            for (int i = 0; i < c.variableIDs.Length; i++)
            {
                // Tested variable
                if (VariablesDictionary[c.variableIDs[i]].name == varID)
                    values[i] = value;
                // Other variables in constraint
                else
                    values[i] = VariablesDictionary[c.variableIDs[i]].value;
            }

            // Check partial solution
            if (!c.Check(values))
            {
                //Debug.Log("Constraint doesn't check out with values " + values);
                return false;
            }
        }

        return true;
    }

    public List<T> GetDifferentValues()
    {
        List<T> different = new List<T>();

        Array.ForEach(Variables, v =>
        {
            if (!different.Contains(v.value))
                different.Add(v.value);
        });

        return different;
    }
}
