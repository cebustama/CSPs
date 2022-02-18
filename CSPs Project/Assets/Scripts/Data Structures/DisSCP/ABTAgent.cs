using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Asynchronous Backtracking Agent Data Structure
/// </summary>
/// <typeparam name="T">Variable domain type</typeparam>
public class ABTAgent<T> : DiSCPAgent<T>
{
    private ABTManager<T> abtManager;

    public ABTAgent(ABTManager<T> manager, string iD, int priority) : base(manager, iD, priority) 
    {
        abtManager = manager;
    }

    /// <summary>
    /// Send "ok?" message ONLY to neighbors of lower priority, containing this agent's variable value
    /// </summary>
    public void SendOK()
    {
        abtManager.SendMessage(
            new ABTMessage<T>(
                DiSCPAgentMessage<T>.MessageType.OK, 
                new List<VariableValuePair<T>>() { new VariableValuePair<T>(Name, value) }
            ),  
            this, Neighbors.ConvertAll(nId => manager.CSP.VariableNames[nId]), 
            (a1, a2) =>
            {
                return (a2.Priority < a1.Priority);
            }
        );
    }

    public bool SendNoGood()
    {
        // COPY VIEW
        List<VariableValuePair<T>> noGood = new List<VariableValuePair<T>>();
        foreach (var pair in View)
        {
            noGood.Add((VariableValuePair<T>)pair);
        }

        if (noGood.Count == 0) return false;

        // Obtain lowest priority variable in nogood and send view as nogood message
        string recipient = noGood.OrderBy(pair => 
        abtManager.GetAgent(pair.Name).Priority).First().Name;

        abtManager.SendMessage(
            new ABTMessage<T>(DiSCPAgentMessage<T>.MessageType.NOGOOD, noGood),
            this, 
            new List<string>() { recipient }
        );

        // Remove chosen nogood variable from view
        for (int i = View.Count - 1; i >= 0; i--)
        {
            VariableValuePair<T> v = (VariableValuePair<T>)View[i];
            if (v.Name == recipient)
            {
                UnityEngine.Debug.Log("<color=magenta>" + Name + " removing " + v.Name + " from view." + "</color>");
                View.RemoveAt(i);
                break;
            }
        }

        return true;
    }

    public void SendAddMeTo(string newID)
    {
        abtManager.SendMessage(
            new ABTMessage<T>(
                DiSCPAgentMessage<T>.MessageType.ADDME, 
                new List<VariableValuePair<T>>() { new VariableValuePair<T>(Name, value) }
            ),
            this, 
            new List<string>() { newID }
        );
    }

    public override bool AssignValue(bool checkConsistency = true)
    {
        return AssignFirstConsistent();
    }
}

public class ABTMessage<T> : DiSCPAgentMessage<T>
{
    public ABTMessage(MessageType t, List<VariableValuePair<T>> pairs)
    {
        Type = t;

        Contents = new List<DiSCPAgentViewTuple<T>>();
        foreach (VariableValuePair<T> p in pairs)
        {
            Contents.Add(p);
        }
    }
}