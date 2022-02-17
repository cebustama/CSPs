using System;
using System.Collections.Generic;
using UnityEngine;

// Handles Asynchronous Backtracking and its agents
public class ABTManager<T> : DiSCPManager<T>
{
    public ABTManager(CSP<T> CSP) : base(CSP) {}

    // TODO: How to move this to DiSCPManager class as a generic type dictionary
    public Dictionary<string, ABTAgent<T>> AgentsIndex;

    public override void InitializeIndex()
    {
        AgentsIndex = new Dictionary<string, ABTAgent<T>>();
    }

    public override DiSCPAgent<T> AddAgent(string id, int priority = 0)
    {
        ABTAgent<T> agent = new ABTAgent<T>(this, id, priority);
        AgentsIndex.Add(id, agent);

        return agent;
    }

    public ABTAgent<T> GetAgent(string id)
    {
        return AgentsIndex[id];
    }

    public override void InitializeAgent(string id)
    {
        GetAgent(id).AssignRandom(rng);
        GetAgent(id).SendOK();
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

    public override int SendMessage(
        DiSCPAgentMessage<T> message,
        DiSCPAgent<T> sender, List<string> recipients,
        Func<DiSCPAgent<T>, DiSCPAgent<T>, bool> condition = null)
    {
        int counter = 0;
        foreach (string r in recipients)
        {
            if (condition == null || condition(sender, AgentsIndex[r]))
            {
                Debug.Log(sender.ID + " (P:" + sender.Priority + ")"
                    + " sent " + message.Print(false) + " to "
                    + AgentsIndex[r].ID + " (P:" + AgentsIndex[r].Priority + ")");

                // Only print nogood for debugging
                if (message.Type == DiSCPAgentMessage<T>.MessageType.NOGOOD)
                    Debug.Log(message.PrintContents());

                AgentsIndex[r].ReceiveMessage(message);
                counter++;
            }
        }

        return counter;
    }

    public override uint CountInconsistencies(string id, T value)
    {
        uint inconsistencies = 0;

        ABTAgent<T> checker = GetAgent(id);

        // AGENT WITH VIEW: Check constraints with neighbors that can be checked
        inconsistencies += CheckView(checker, value);

        // TEST: Check only nogoods with this value included
        List<List<DiSCPAgentViewTuple<T>>> toCheck = new List<List<DiSCPAgentViewTuple<T>>>();
        foreach (List<DiSCPAgentViewTuple<T>> ng in checker.NoGoods)
        {
            foreach (DiSCPAgentViewTuple<T> tuple in ng)
            {
                if (tuple.ID == checker.ID) toCheck.Add(new List<DiSCPAgentViewTuple<T>>() { tuple });
            }
        }

        // NOGOODS
        inconsistencies += CheckNoGoods(checker, value
            , toCheck
            );

        return inconsistencies;
    }

    public override void CheckSolutionFound()
    {
        bool found = true;
        foreach (DiSCPAgent<T> agent in AgentsIndex.Values)
        {
            if (!agent.Consistent)
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

    public override void Stop()
    {
        Stopped = true;
        foreach (DiSCPAgent<T> agent in AgentsIndex.Values)
        {
            agent.Stop();
        }
    }

    public override void AddNeighbors(string a1ID, string a2ID)
    {
        //throw new NotImplementedException();
    }

    public override bool AssignValue(string aID, bool checkConsistency = true)
    {
        throw new NotImplementedException();
    }
}
