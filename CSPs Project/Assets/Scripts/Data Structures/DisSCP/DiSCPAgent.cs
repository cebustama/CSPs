using System.Collections.Generic;
using UnityEngine;

// TODO: Crear Agent genérico, DisSCP y DisCOP agents deberían extender esa clase

/// <summary>
/// Distributed CSP solver agent
/// TODO: where T: IPrint o con mejor nombre
/// </summary>
/// <typeparam name="T">Variable value type</typeparam>
public abstract class DiSCPAgent<T>
{
    protected DiSCPManager<T> manager;

    [SerializeField]
    public string ID { get; private set; }

    // Get value directly from CSP
    public T value => manager.CSP.VariablesDictionary[ID].value;

    [SerializeField]
    public int Priority { get; private set; }

    [SerializeField]
    public List<string> Neighbors = new List<string>();

    // TODO: Should be dictionary with varID as index
    public List<DiSCPAgentViewTuple<T>> View = new List<DiSCPAgentViewTuple<T>>();

    // TODO: Index for faster access
    public List<List<DiSCPAgentViewTuple<T>>> NoGoods = new List<List<DiSCPAgentViewTuple<T>>>();

    public Queue<DiSCPAgentMessage<T>> Messages = new Queue<DiSCPAgentMessage<T>>();

    public bool Consistent { get; private set; }

    public bool Stopped { get; private set; }

    public DiSCPAgent(DiSCPManager<T> manager, string iD, int priority)
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

    public void AddToView(DiSCPAgentViewTuple<T> t)
    {
        for (int i = 0; i < View.Count; i++)
        {
            DiSCPAgentViewTuple<T> e = View[i];
            // Replace value and stop if already in view
            if (e.ID == t.ID)
            {
                View[i] = t;
                return;
            }
        }

        View.Add(t);
    }

    public DiSCPAgentViewTuple<T> GetViewValue(string ID)
    {
        foreach (var tuple in View)
            if (tuple.ID == ID) return tuple;

        return null;
    }

    public void AddNoGood(List<DiSCPAgentViewTuple<T>> contents)
    {
        if (!NoGoods.Contains(contents)) NoGoods.Add(contents);
    }

    public void AddNeighbor(string nID)
    {
        if (!Neighbors.Contains(ID))
            Neighbors.Add(nID);

        //manager.AddNeighbors(ID, nID);
    }

    public abstract bool AssignValue(bool checkConsistency = true);

    public void AssignRandom(System.Random rng) => 
        manager.CSP.AssignRandom(ID, rng);

    public bool AssignFirstConsistent()
    {
        return manager.AssignFirstConsistent(this);
    }

    public bool IsViewConsistent()
    {
        return (manager.CountInconsistencies(ID, value) == 0);
    }

    public void SetConsistent(bool value)
    {
        //Debug.LogError("Agent " + ID + " setting Consistent to " + value);
        Consistent = value;

        if (Consistent) manager.CheckSolutionFound();
    }

    public void SetPriority(int p)
    {
        Debug.Log("<color=cyan>" + ID + " assigning new priority " + p + "</color>");
        Priority = p;
    }

    public void ReceiveMessage(DiSCPAgentMessage<T> message) =>
        Messages.Enqueue(message);

    public void SendNoSolution() =>
        manager.Stop();

    public void Stop() => Stopped = true;

    public string PrintView()
    {
        string printable = "";
        foreach (var tuple in View)
        {
            printable += " " + tuple.Print();
        }

        return printable;
    }
}

public class DiSCPAgentViewTuple<T>
{
    public string ID;
    public T value;
    public uint priority;

    public DiSCPAgentViewTuple(string iD, T value, uint priority = 0)
    {
        ID = iD;
        this.value = value;
        this.priority = priority;
    }

    public string Print()
    {
        string printable = ID + " " + value.ToString();
        if (priority > 0) printable += " P:" + priority;
        return printable;
    }
}

public class VariableValuePair<T> : DiSCPAgentViewTuple<T>
{
    public VariableValuePair(string iD, T value) : base(iD, value) { }
}

public class DiSCPAgentMessage<T>
{
    public string senderID;

    public enum MessageType
    {
        OK,
        NOGOOD,
        ADDME
    }

    public MessageType Type;

    public List<DiSCPAgentViewTuple<T>> Contents;

    public virtual string Print(bool includeContent = true)
    {
        string printable = Type.ToString();

        // Apply color to message type
        switch (Type)
        {
            case MessageType.OK:
                printable = "<color=green>" + printable + "</color>";
                break;
            case MessageType.NOGOOD:
                printable = "<color=red>" + printable + "</color>";
                break;
        }

        if (includeContent) printable += PrintContents();

        return printable;
    }

    public virtual string PrintContents()
    {
        string printable = "";
        foreach (var tuple in Contents)
            printable += " " + tuple.Print();
        return printable;
    }
}