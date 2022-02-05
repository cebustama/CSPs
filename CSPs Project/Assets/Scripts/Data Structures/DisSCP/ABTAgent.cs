using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Asynchronous Backtracking Agent Data Structure
/// </summary>
/// <typeparam name="T">Variable domain type</typeparam>
[System.Serializable]
public class ABTAgent<T>
{
    public struct VariableValuePair
    {
        public string varID;
        public T value; 

        public VariableValuePair(string vID, T value)
        {
            this.varID = vID;
            this.value = value;
        }
    }

    public struct ABTMessage
    {
        public enum MessageType
        {
            OK,
            NOGOOD,
            ADDME
        }

        public MessageType Type;

        public List<VariableValuePair> VarValues;

        public string Print()
        {
            string finalString = Type.ToString();

            string color =
                (Type == MessageType.OK) ? "green" :
                (Type == MessageType.NOGOOD) ? "red" :
                "blue";


            finalString = "<color=" + color + ">" + finalString + "</color>";
            /*finalString += " {";
            foreach (VariableValuePair p in VarValues)
            {
                finalString += "(" + p.varID + "," + p.value + ")";
            }
            finalString += "}";*/

            return finalString;
        }
    }

    // Send messages through Manager Class
    private ABTManager<T> manager;

    [SerializeField]
    public string ID { get; private set; }

    public T value => manager.CSP.VariablesDictionary[ID].value;

    [SerializeField]
    public int Priority { get; private set; }

    [SerializeField]
    public List<string> Neighbors = new List<string>();

    public List<VariableValuePair> View = new List<VariableValuePair>();

    public List<List<VariableValuePair>> NoGoods = new List<List<VariableValuePair>>();

    public Queue<ABTMessage> Messages = new Queue<ABTMessage>();

    public bool Consistent { get; private set; }

    public bool Stopped { get; private set; }

    public ABTAgent(ABTManager<T> manager, string varID, int varPriority)
    {
        this.manager = manager;
        ID = varID;
        Priority = varPriority;

        // Obtain logical neighbors list from CSP (connected by a constraint)
        foreach (var c in manager.CSP.ConstraintsDictionary[varID])
        {
            foreach (string v in c.variableIDs)
                if (v != varID && !Neighbors.Contains(v)) Neighbors.Add(v);
        }

        View = new List<VariableValuePair>();
        NoGoods = new List<List<VariableValuePair>>();
    }

    public void AddNeighbor(string nID)
    {
        if (!Neighbors.Contains(ID))
            Neighbors.Add(nID);

        manager.AddNeighbors(ID, nID);
    }

    public void AssignRandom(System.Random rng)
    {
        manager.CSP.AssignRandom(ID, rng);
        //Debug.Log("Agent Variable " + ID + " set it's value to " + manager.CSP.VariablesDictionary[ID].value);
    }

    public bool AssignFirstConsistent()
    {
        return manager.AssignFirstConsistent(this);
    }

    /// <summary>
    /// Send "ok?" message ONLY to neighbors of lower priority, containing this agent's variable value
    /// </summary>
    public void SendOK()
    {
        manager.SendMessage(
            new ABTMessage() 
            { 
                Type = ABTMessage.MessageType.OK, 
                VarValues = new List<VariableValuePair>() { new VariableValuePair(ID, value) } 
            },  
            this, Neighbors, 
            (a1, a2) =>
            {
                return (a2.Priority < a1.Priority);
            }
        );
    }

    public bool SendNoGood()
    {
        // COPY VIEW
        List<VariableValuePair> noGood = new List<VariableValuePair>(View);

        if (noGood.Count == 0) return false;

        // Obtain lowest priority variable in view and send view as nogood message
        string recipient = View.OrderBy(agent => manager.AgentsIndex[agent.varID].Priority).First().varID;

        // Remove chosen nogood variable from view
        for (int i = View.Count - 1; i >= 0; i--)
        {
            VariableValuePair v = View[i];
            if (v.varID == recipient)
            {
                View.RemoveAt(i);
                break;
            }
        }

        manager.SendMessage(
            new ABTMessage()
            {
                Type = ABTMessage.MessageType.NOGOOD,
                VarValues = noGood
            },
            this, 
            new List<string>() { recipient }
        );

        return true;
    }

    public void SendAddMeTo(string newID)
    {
        manager.SendMessage(
            new ABTMessage()
            {
                Type = ABTMessage.MessageType.ADDME,
                VarValues = new List<VariableValuePair>() { new VariableValuePair(ID, value) }
            },
            this, 
            new List<string>() { newID }
        );
    }

    public void SendNoSolution()
    {
        manager.Stop();
    }

    public void ReceiveMessage(ABTMessage message)
    {
        Messages.Enqueue(message);
    }

    public bool IsViewConsistent()
    {
        return manager.IsConsistentWithMe(this, value);
    }

    public void Stop()
    {
        Stopped = true;
    }

    public void SetConsistent(bool value)
    {
        Consistent = value;

        if (Consistent) manager.CheckSolutionFound();
    }
}
