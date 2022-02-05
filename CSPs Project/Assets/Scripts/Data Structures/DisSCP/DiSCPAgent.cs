using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DiSCPAgent<T, M>
    where M : struct, IConvertible
{
    private DiSCPManager<T, M> manager;

    [SerializeField]
    public string ID { get; private set; }

    // Get value directly from CSP
    public T value => manager.CSP.VariablesDictionary[ID].value;

    [SerializeField]
    public int Priority { get; private set; }

    [SerializeField]
    public List<string> Neighbors = new List<string>();

    public List<DiSCPAgentViewTuple<T>> View = new List<DiSCPAgentViewTuple<T>>();

    public List<List<DiSCPAgentViewTuple<T>>> NoGoods = new List<List<DiSCPAgentViewTuple<T>>>();

    public Queue<DiSCPAgentMessage<T, M>> Messages = new Queue<DiSCPAgentMessage<T, M>>();

    public bool Consistent { get; private set; }

    public bool Stopped { get; private set; }

    public DiSCPAgent(DiSCPManager<T, M> manager, string iD, int priority)
    {
        this.manager = manager;
        ID = iD;
        Priority = priority;

        // Obtain logical neighbors list from CSP (connected by a constraint)
        foreach (var c in manager.CSP.ConstraintsDictionary[ID])
        {
            foreach (string v in c.variableIDs)
                if (v != ID && !Neighbors.Contains(v)) Neighbors.Add(v);
        }

        View = new List<DiSCPAgentViewTuple<T>>();
        NoGoods = new List<List<DiSCPAgentViewTuple<T>>>();
    }

    public void AddNeighbor(string nID)
    {
        if (!Neighbors.Contains(ID))
            Neighbors.Add(nID);

        manager.AddNeighbors(ID, nID);
    }
}

public struct DiSCPAgentViewTuple<T>
{
    public string ID;
    public T value;

    public DiSCPAgentViewTuple(string iD, T value)
    {
        ID = iD;
        this.value = value;
    }
}

public class DiSCPAgentMessage<T, M> 
    where M: struct, IConvertible
{
    public M Type;

    public List<DiSCPAgentViewTuple<T>> Items;

    public virtual string Print()
    {
        return Type.ToString();
    }
}