using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Distributed CSP solver manager
/// </summary>
/// <typeparam name="T">Variable value type/data structure</typeparam>
public abstract class DiSCPManager<T>
{
    protected static System.Random rng = new System.Random();

    public CSP<T> CSP { get; private set; }

    public bool Stopped;

    public bool FoundSolution;

    public Dictionary<string, DiSCPAgent<T>> testDictionary;

    public DiSCPManager(CSP<T> CSP)
    {
        this.CSP = CSP;
        InitializeIndex();
    }

    public abstract void InitializeIndex();

    public abstract DiSCPAgent<T> AddAgent(string id, int priority = 0);

    public abstract void InitializeAgent(string id);

    public abstract uint CountInconsistencies(string id, T value);

    protected virtual uint CheckView(DiSCPAgent<T> checker, T value,
        List<DiSCPAgentViewTuple<T>> options = null)
    {
        Debug.Log("<color=cyan>" + "Checking view with value " + value + "</color>");

        List<DiSCPAgentViewTuple<T>> toCheck = (options == null) ?
            checker.View : options;

        uint count = 0;
        foreach (DiSCPAgentViewTuple<T> v in toCheck)
        {
            var constraints = CSP.GetConstraintsFromTo(checker.Name, v.Name);
            if (constraints.Count > 0) Debug.Log("Checking (" + checker.Name + "," 
                + value + " with (" + v.Name + "," + v.value + ") constraints: " 
                + (constraints.Count));

            uint currCount = count;
            // Check that every shared constraint is valid
            foreach (var c in constraints)
            {
                if (!c.Check(new T[] { value, v.value }))
                {
                    Debug.Log("<color=red>INCONSISTENCE FOUND " + c.variableIDs[0] + " " + c.variableIDs[1] + "</color>");
                    count++;
                }
            }

            //if (constraints.Count > 0 && currCount == count) Debug.Log("No problem.");
        }

        return count;
    }

    protected uint CheckNoGoods(DiSCPAgent<T> checker, T value, 
        List<List<DiSCPAgentViewTuple<T>>> options = null)
    {
        List<List<DiSCPAgentViewTuple<T>>> toCheck = (options == null) ?
            checker.NoGoods : options;

        if (toCheck.Count > 0)
            Debug.Log("<color=orange>CHECKING NOGOODS</color> with value " + value);

        uint count = 0;
        foreach (var pairs in toCheck)
        {
            foreach (var ng in pairs)
            {
                // Check own value first, skip to next
                if (ng.Name == checker.Name)
                {
                    if (ng.value.Equals(value))
                    {
                        count++;
                        Debug.Log("<color=yellow>COMPATIBLE WITH OWN VALUE</color>");
                    }
                    continue;
                }

                var viewItem = checker.GetViewValue(ng.Name);

                if (viewItem == null)
                {
                    //Debug.Log("No value in view for " + ng.ID);
                    continue;
                }
                
                var viewValue = viewItem.value;

                Debug.Log("Checking NOGOOD PAIR (" + ng.Name + "," + ng.value + ") " +
                    "with view value (" + ng.Name + "," + viewValue + ")");

                if (ng.value.Equals(viewValue))
                {
                    Debug.Log("<color=yellow>COMPATIBLE</color>");
                    count++;
                }
            }
        }

        return count;
    }

    public abstract void AddNeighbors(string a1ID, string a2ID);

    public abstract void Start(string seed);

    /// <summary>
    /// Sends a message from agent to other according to condition function
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="recipients"></param>
    /// <param name="condition"></param>
    /// <returns>Number of messages sent</returns>
    public abstract int SendMessage(
        DiSCPAgentMessage<T> message,
        DiSCPAgent<T> sender, List<string> recipients,
        Func<DiSCPAgent<T>, DiSCPAgent<T>, bool> condition = null);

    public bool AssignFirstConsistent(DiSCPAgent<T> checker)
    {
        Debug.Log("<color=magenta>" + checker.Name + 
            " trying to find value in domain." + "</color>");
        COP<T>.Variable<T> variable = CSP.GetVariable(checker.ID);

        // Check every element in domain until one is consistent
        foreach (T v in variable.domain)
        {
            if (!v.Equals(checker.value))
            {
                //Debug.Log("<color=orange>" + checker.ID + " trying " + v + "</color>");
                if (CountInconsistencies(checker.Name, v) == 0)
                {
                    CSP.AssignValue(checker.Name, v);
                    return true;
                }
            }
        }

        return false;
    }

    public abstract bool AssignValue(string aID, bool checkConsistency = true);

    public abstract void CheckSolutionFound();

    public abstract void Stop();
}
