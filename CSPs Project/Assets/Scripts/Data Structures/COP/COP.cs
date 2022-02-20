
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[Serializable]
/// <summary>
/// Constraint Optimization Problem
/// Tupla (X,D,R)
/// X={X1,...,Xn} es un conjunto de variables
/// D={d1,...,dn} es un conjunto de dominios de variables discretas y finitas.
/// R={r1,...,rm} es un conjunto de funciones de utilidad, donde cada ri:di1×...×dik→R, 
/// que asigna una utilidad (recompensa) a cada combinación posible de valores 
/// de las variables. Las cantidades negativas significan costes.
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class COP<T>
{
    protected static System.Random rng = new System.Random();

    // Variables list
    public Variable<T>[] Variables { get; private set; }

    // Store variable indices
    // {key: hash, value: variable index in list}
    public Dictionary<int, int> VariablesIndex { get; private set; }

    // In case you want to use variable names
    // {key: hash, value: name}
    public Dictionary<int, string> VariableNames { get; private set; }

    // TODO: IMPLEMENT CONSTRAINTS AS UTILITY FUNCTIONS, MAYBE USE DELEGATE AND SUBSCRIBE EACH CONSTRAINT TO BASE FUNCTION
    public List<Constraint<T>> Constraints { get; private set; }
    // TODO: Store index instead of constraint
    public Dictionary<int, List<Constraint<T>>> ConstraintsDictionary { get; private set; }

    // TODO: Should not be here, change to NumDifferentValues or something
    public uint NumColors { get; private set; }

    public COP(string[] names, T[][] domains, T defaultValue = default(T))
    {
        Variables = new Variable<T>[names.Length];
        VariablesIndex = new Dictionary<int, int>();
        VariableNames = new Dictionary<int, string>();
        for (int i = 0; i < Variables.Length; i++)
        {
            Variables[i] = new Variable<T>()
            {
                id = names[i].GetHashCode(),
                domain = domains[i].ToList(),
                value = defaultValue
            };

            // Store variable index in list
            VariablesIndex.Add(Variables[i].id, i);
            VariableNames.Add(Variables[i].id, names[i]);
        }

        Constraints = new List<Constraint<T>>();
        ConstraintsDictionary = new Dictionary<int, List<Constraint<T>>>();

        // Print Variables TODO: Move to method
        string printable = "";
        foreach (var v in Variables)
        {
            printable += "V(" +
                "id: " + v.id + " " +
                "index: " + VariablesIndex[v.id] + " " +
                    "(" + (Variables[VariablesIndex[v.id]].id == v.id) + ") " +
                "name: " + VariableNames[v.id] + ")";
        }
        Debug.Log(printable);

        NumColors = 0;
    }

    public Variable<T> GetVariable(string name)
    {
        //Debug.Log("Looking for " + name + " id:" + name.GetHashCode());
        return Variables[VariablesIndex[name.GetHashCode()]];
    }

    public Variable<T> GetVariable(int id)
    {
        return Variables[VariablesIndex[id]];
    }

    public void SetNumColors(uint num)
    {
        if (num == 0) throw new ArgumentOutOfRangeException();

        NumColors = num;
    }

    public virtual void Solve() { }

    public virtual void SolveWithAgents(GameObject[] agents, string seed) { }

    public virtual void Step() { }

    public virtual List<Variable<T>> OrderVariables(Func<List<Variable<T>>, List<Variable<T>>> orderingMethod)
    {
        return orderingMethod(Variables.ToList());
    }

    public virtual bool AssignFirstValid(string varName, bool addValuesToDomain = false, Func<T> randomFunction = null)
    {
        Variable<T> variable = GetVariable(varName);

        foreach (T value in variable.domain)
        {
            // Found valid value in domain
            if (IsConsistent(varName, value))
            {
                variable.AssignValue(value);
                return true;
            }
        }

        // if no values in domain were cosistent, add new one
        if (addValuesToDomain)
        {
            if (randomFunction == null)
                throw new ArgumentException();

            variable.AssignValue(randomFunction());
        }

        return false;
    }

    public virtual bool AssignValidOtherThan(string varName, T exception)
    {
        Variable<T> variable = GetVariable(varName);

        List<T> randomizedDomain = variable.
            domain.OrderBy(a => rng.Next()).ToList();

        foreach (T value in randomizedDomain)
        {
            // Found valid value in domain
            if (!value.Equals(exception) && IsConsistent(varName, value))
            {
                variable.AssignValue(value);
                return true;
            }
        }

        return false;
    }

    public virtual void AssignRandom(string varName, System.Random rng)
    {
        GetVariable(varName).AssignRandom(rng);
    }

    public virtual void AssignValue(string varName, T value)
    {
        GetVariable(varName).AssignValue(value);
    }

    // Checks solution for consistency
    public virtual bool IsConsistent(string varName, T value)
    {
        Variable<T> variable = GetVariable(varName);

        // Variable has no constraints
        if (!ConstraintsDictionary.ContainsKey(variable.id)) return true;

        //Debug.Log("Checking " + varID + " with value " + value.ToString() + " for " + ConstraintsDictionary[varID].Count + " constraints.");
        // Check all constraints that involve variable
        foreach (Constraint<T> c in ConstraintsDictionary[variable.id])
        {
            T[] values = new T[c.variableIDs.Length];
            // Get involved variables and store involved values, except the one being tested
            for (int i = 0; i < c.variableIDs.Length; i++)
            {
                // Tested variable
                if (GetVariable(c.variableIDs[i]).id == variable.id)
                    values[i] = value;
                // Other variables in constraint
                else
                    values[i] = GetVariable(c.variableIDs[i]).value;
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

    // TODO: Use
    public string GetVariableName(int id)
    {
        return VariableNames[id];
    }
}
