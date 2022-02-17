using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AWCSManager<T> : DiSCPManager<T>
{
    public AWCSManager(CSP<T> CSP) : base(CSP) {}

    public Dictionary<string, AWCSAgent<T>> AgentsIndex;

    public override DiSCPAgent<T> AddAgent(string id, int priority = 0)
    {
        AWCSAgent<T> agent = new AWCSAgent<T>(this, id);
        AgentsIndex.Add(id, agent);

        return agent;
    }

    public AWCSAgent<T> GetAgent(string id)
    {
        return AgentsIndex[id];
    }

    public override void AddNeighbors(string a1ID, string a2ID)
    {
        throw new NotImplementedException();
    }

    public override void CheckSolutionFound()
    {
        bool found = true;

        /*foreach (DiSCPAgent<T> agent in AgentsIndex.Values)
        {
            found = agent.Consistent;
        }*/

        // Check that no agents have messages
        foreach (var agent in AgentsIndex.Values)
        {
            if (agent.Messages.Count > 0)
            {
                found = false;
                break;
            }
        }

        if (found)
        {
            Debug.Log("Solution found!");
            Stop();
        }
    }

    public override void InitializeAgent(string id)
    {
        GetAgent(id).AssignRandom(rng);
        GetAgent(id).SendOK();
    }

    public override void InitializeIndex()
    {
        AgentsIndex = new Dictionary<string, AWCSAgent<T>>();
    }

    public override uint CountInconsistencies(string id, T value)
    {
        uint inconsistencies = 0;

        AWCSAgent<T> checker = GetAgent(id);

        // Obtain list of agents in VIEW with higher priorities
        List<List<DiSCPAgentViewTuple<T>>> higherView = GetHigherAgents(checker,
            new List<List<DiSCPAgentViewTuple<T>>>() { checker.View });

        // AGENT WITH VIEW: Check constraints with neighbors WITH HIGHER PRIORITY
        if (higherView[0].Count > 0) inconsistencies += CheckView(checker, value, higherView[0]);
        else Debug.Log("<color=cyan>" + checker.ID + " has no variables in view of higher priority." + "</color>");

        // Obtain list of agents in NOGOODS with lower priorities
        List<List<DiSCPAgentViewTuple<T>>> higherNoGoods = GetHigherAgents(checker, checker.NoGoods);

        // NOGOODS
        if (higherNoGoods.Count > 0) inconsistencies += CheckNoGoods(checker, value);
        else Debug.Log("<color=cyan>" + checker.ID + " has no variables in no good list of lower priority." + "</color>");

        Debug.Log("Inconsistencies: " + inconsistencies);

        return inconsistencies;
    }

    /// <summary>
    /// Returns only agents of higher priority or higher id (alphabetical)
    /// </summary>
    /// <param name="checker"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private List<List<DiSCPAgentViewTuple<T>>> GetHigherAgents(
        AWCSAgent<T> checker, List<List<DiSCPAgentViewTuple<T>>> options)
    {
        List<List<DiSCPAgentViewTuple<T>>> candidates = new List<List<DiSCPAgentViewTuple<T>>>();

        foreach (var tupleList in options)
        {
            candidates.Add(new List<DiSCPAgentViewTuple<T>>());
            foreach (var tuple in tupleList)
            {
                // Compare priorities
                if (AgentsIndex[tuple.ID].Priority > checker.Priority)
                {
                    candidates[candidates.Count - 1].Add(tuple);
                    continue;
                }

                // If same priority, compare alphabetically
                if (AgentsIndex[tuple.ID].Priority == checker.Priority)
                {
                    if (string.Compare(tuple.ID, checker.ID,
                        StringComparison.CurrentCultureIgnoreCase) > 0)
                    {
                        candidates[candidates.Count - 1].Add(tuple);
                        continue;
                    }
                }
            }
        }

        return candidates;
    }

    /// <summary>
    /// Returns only agents of lower priority or lower id (alphabetical)
    /// </summary>
    /// <param name="checker"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public List<List<DiSCPAgentViewTuple<T>>> GetLowerAgents(
        AWCSAgent<T> checker, List<List<DiSCPAgentViewTuple<T>>> options)
    {
        List<List<DiSCPAgentViewTuple<T>>> candidates = new List<List<DiSCPAgentViewTuple<T>>>();

        foreach (var tupleList in options)
        {
            candidates.Add(new List<DiSCPAgentViewTuple<T>>());
            foreach (var tuple in tupleList)
            {
                // Compare priorities
                if (AgentsIndex[tuple.ID].Priority < checker.Priority)
                {
                    candidates[candidates.Count - 1].Add(tuple);
                    continue;
                }

                // If same priority, compare alphabetically
                if (AgentsIndex[tuple.ID].Priority == checker.Priority)
                {
                    if (string.Compare(tuple.ID, checker.ID,
                        StringComparison.CurrentCultureIgnoreCase) < 0)
                    {
                        candidates[candidates.Count - 1].Add(tuple);
                        continue;
                    }
                }
            }
        }

        return candidates;
    }

    public override int SendMessage(DiSCPAgentMessage<T> message, DiSCPAgent<T> sender, 
        List<string> recipients, Func<DiSCPAgent<T>, DiSCPAgent<T>, bool> condition = null)
    {
        int counter = 0;
        foreach (string r in recipients)
        {
            if (condition == null || condition(sender, AgentsIndex[r]))
            {
                Debug.Log(sender.ID + " (P:" + sender.Priority + ")"
                    + " sent " + message.Print(false) + " to "
                    + AgentsIndex[r].ID + " (P:" + AgentsIndex[r].Priority + ")");

                AgentsIndex[r].ReceiveMessage(message);
                counter++;
            }
        }

        return counter;
    }

    public override void Start(string seed)
    {
        rng = new System.Random(seed.GetHashCode());

        Stopped = false;
        FoundSolution = false;

        // Start with random value
        foreach (DiSCPAgent<T> a in AgentsIndex.Values)
        {
            InitializeAgent(a.ID);
        }
    }

    public override void Stop()
    {
        Stopped = true;
        foreach (DiSCPAgent<T> agent in AgentsIndex.Values)
        {
            agent.Stop();
        }
    }

    public override bool AssignValue(string aID, bool checkConsistency = true)
    {
        AWCSAgent<T> checker = AgentsIndex[aID];

        // Obtain CSPVariable
        CSP<T>.CSPVariable<T> variable = CSP.VariablesDictionary[checker.ID];

        List<T> toCheck = variable.domain;
        // Get consistent values in domain (should exclude current value)
        if (checkConsistency)
        {
            List<T> consistentValues = GetConsistentValues(variable.domain, variable);
            if (consistentValues.Count == 0)
            {
                Debug.Log("<color=red>" + "No consistent values in domain for " + aID + "</color>");
                return false;
            }
            toCheck = consistentValues;
        }

        // Get neighbors with less priority
        List<string> lowerIDs = GetLowerNeighbors(checker);
        if (lowerIDs.Count == 0)
        {
            Debug.Log("<color=cyan>No lower priority neighbors for " + aID + "</color>");
        }

        // Find CONSISTENT value with the least restrictions violated with lower priority neighbors
        var allRestrictions = CSP.ConstraintsDictionary[aID];
        int minRestrictions = int.MaxValue;
        T chosenValue = default(T);
        foreach (T value in toCheck)
        {
            int violated = 0;
            foreach (var restriction in allRestrictions)
            {
                // Check every restriction shared with lower neighbors
                if (lowerIDs.Contains(restriction.variableIDs[1]))
                {
                    if (!restriction.Check(new T[] { value, 
                        AgentsIndex[restriction.variableIDs[1]].value }))
                    {
                        violated++;
                    }
                }
            }

            // Update values
            if (violated < minRestrictions)
            {
                minRestrictions = violated;
                chosenValue = value;
            }
        }

        CSP.AssignValue(aID, chosenValue);
        return true;
    }

    /// <summary>
    /// Find consistent value for variable between options, avoid checking current
    /// </summary>
    /// <param name="options"></param>
    /// <param name="variable"></param>
    /// <returns></returns>
    private List<T> GetConsistentValues(List<T> options, CSP<T>.CSPVariable<T> variable)
    {
        Debug.Log("<color=yellow>" + "Finding consistent values for " + variable.name + "</color>");
        List<T> values = new List<T>();
        foreach (T v in options)
        {
            if (!v.Equals(variable.value)) Debug.Log("<color=yellow>" + "Trying " + v + "</color>");

            if (!v.Equals(variable.value) && 
                CountInconsistencies(variable.name, v) == 0)
            {
                values.Add(v);
            }
        }

        return values;
    }

    private List<string> GetLowerNeighbors(AWCSAgent<T> checker)
    {
        List<string> lowerIDs = new List<string>();
        foreach (string nID in checker.Neighbors)
        {
            if (AgentsIndex[nID].Priority <= checker.Priority)
            {
                // If same priority, compare ID alphabetically
                if (AgentsIndex[nID].Priority == checker.Priority)
                {
                    if (string.CompareOrdinal(nID, checker.ID) < 0)
                        lowerIDs.Add(nID);
                }
                else
                    lowerIDs.Add(nID);
            }
        }

        return lowerIDs;
    }
}
