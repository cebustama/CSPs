using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiSCPManager<T, M>
    where M: struct, IConvertible
{
    protected static System.Random rng = new System.Random();

    public CSP<T> CSP { get; private set; }

    // Manager stores direct references to ABTAgents
    public Dictionary<string, DiSCPAgent<T, M>> AgentsIndex;

    public bool Stopped { get; private set; }

    public bool FoundSolution { get; private set; }

    public DiSCPManager(CSP<T> CSP)
    {
        this.CSP = CSP;
        AgentsIndex = new Dictionary<string, DiSCPAgent<T, M>>();
    }

    public DiSCPAgent<T, M> AddAgent(string varID, int priority = 0)
    {
        DiSCPAgent<T, M> agent = new DiSCPAgent<T, M>(this, varID, priority);
        AgentsIndex.Add(varID, agent);

        return agent;
    }

    public void AddNeighbors(string a1ID, string a2ID)
    {
        //CSP.Constraints
    }

    public void Start(string seed)
    {
        rng = new System.Random(seed.GetHashCode());

        Stopped = false;
        FoundSolution = false;

        // Start with random value
        foreach (DiSCPAgent<T, M> a in AgentsIndex.Values)
        {
            InitializeAgent(a);
        }    
    }

    public virtual void InitializeAgent(DiSCPAgent<T, M> a)
    {
        //a.AssignRandom(rng);
        //a.SendOK();
    }
}
