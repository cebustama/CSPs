using System;
using System.Collections.Generic;
using UnityEngine;


// Handles Asynchronous Backtracking and its agents
public class ABTManager<T>
{
    protected static System.Random rng = new System.Random();

    public CSP<T> CSP { get; private set; }

    // Manager stores direct references to ABTAgents
    public Dictionary<string, ABTAgent<T>> AgentsIndex;

    public bool Stopped { get; private set; }

    public bool FoundSolution { get; private set; }

    public ABTManager(CSP<T> CSP) 
    {
        this.CSP = CSP;
        AgentsIndex = new Dictionary<string, ABTAgent<T>>();
    }

    public ABTAgent<T> AddAgent(string varID, int priority)
    {
        ABTAgent<T> agent = new ABTAgent<T>(this, varID, priority);
        AgentsIndex.Add(varID, agent);

        return agent;
    }

    public void Start(string seed)
    {
        rng = new System.Random(seed.GetHashCode());

        Stopped = false;
        FoundSolution = false;

        // Start with random value
        foreach (ABTAgent<T> a in AgentsIndex.Values)
        {
            a.AssignRandom(rng);
            a.SendOK();
        }
    }

    /// <summary>
    /// Sends a message from agent to other according to condition function
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="recipients"></param>
    /// <param name="condition"></param>
    /// <returns>Number of messages sent</returns>
    public int SendMessage(
        ABTAgent<T>.ABTMessage message,
        ABTAgent<T> sender, List<string> recipients, 
        Func<ABTAgent<T>, ABTAgent<T>, bool> condition = null)
    {
        int counter = 0;
        foreach (string r in recipients)
        {
            if (condition == null || condition(sender, AgentsIndex[r]))
            {
                Debug.Log(sender.ID + " (P:" + sender.Priority + ")"
                    + " sent " + message.Print() + " to " 
                    + AgentsIndex[r].ID + " (P:" + AgentsIndex[r].Priority + ")");

                AgentsIndex[r].ReceiveMessage(message);
                counter++;
            }
        }

        return counter;
    }

    public bool IsConsistentWithMe(ABTAgent<T> checker, T value)
    {
        bool consistent = true;

        // AGENT WITH VIEW: Check constraints with neighbors that can be checked
        foreach (var v in checker.View)
        {
            var constraints = CSP.GetConstraintsFromTo(checker.ID, v.varID);
            if (constraints.Count > 0) Debug.Log("Checking (" + checker.ID + "," + value + " with (" + v.varID + "," + v.value + ") constraints: " + (constraints.Count));

            // Check that every shared constraint is valid
            foreach (var c in constraints)
            {
                consistent = c.Check(new T[] { value, v.value });
                if (!consistent)
                {
                    Debug.Log("INCONSISTENCE FOUND " + c.variableIDs[0] + " " + c.variableIDs[1]);
                    return false;
                }
            }
        }

        // NEIGHBORS WITH NEIGHBORS: Check constraints between variables in view
        /*foreach (var v1 in checker.View)
        {
            foreach (var v2 in checker.View)
            {
                if (!v1.Equals(v2))
                {
                    var constraints = CSP.GetConstraintsFromTo(v1.varID, v2.varID);
                    if (constraints.Count > 0) Debug.Log("Checking (" + v1.varID + "," + v1.value + " with (" + v2.varID + "," + v2.value + ") constraints: " + (constraints.Count));
                    foreach (var c in constraints)
                    {
                        consistent = c.Check(new T[] { v1.value, v2.value });
                        if (!consistent) return false;
                    }
                }
            }
        }*/

        // NOGOODS
        foreach (var pairs in checker.NoGoods)
        {
            foreach (var ng in pairs)
            {
                // Get shared constraints with nogood variable
                Debug.Log("Checking NOGOOD PAIR (" + ng.varID + "," + ng.value + ") with own value (" + checker.ID + "," + checker.value + ")");
                if (ng.varID == checker.ID && ng.value.Equals(checker.value)) return false;

                var constraints = CSP.GetConstraintsFromTo(checker.ID, ng.varID);
                foreach (var c in constraints)
                {
                    // Assure that combination is not valid (breaks constraint)
                    consistent = !c.Check(new T[] { value, ng.value });
                    if (!consistent)
                    {
                        Debug.Log("NO GOOD PROBLEM");
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public bool AssignFirstConsistent(ABTAgent<T> checker)
    {
        Debug.Log(checker.ID + " trying to find value in domain.");
        var variable = CSP.VariablesDictionary[checker.ID];

        // Check every element in domain until one is consistent
        foreach (T v in variable.domain)
        {
            if (!v.Equals(checker.value))
            {
                if (IsConsistentWithMe(checker, v))
                {
                    CSP.AssignValue(checker.ID, v);
                    return true;
                }
            }
        }

        return false;
    }

    public void Stop()
    {
        Stopped = true;
        foreach (ABTAgent<T> agent in AgentsIndex.Values)
        {
            agent.Stop();
        }
    }

    public void AddNeighbors(string a1ID, string a2ID)
    {
        //CSP.Constraints
    }

    public void CheckSolutionFound()
    {
        bool found = true;
        foreach (ABTAgent<T> agent in AgentsIndex.Values)
        {
            found = agent.Consistent;
        }

        if (found)
        {
            Debug.Log("Solution found!");
            Stop();
        }
    }
}
