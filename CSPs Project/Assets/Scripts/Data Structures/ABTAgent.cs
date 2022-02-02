
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ABTAgent<T>
{
    public struct VariableValuePair
    {
        public string varID;
        public T value; 
    }

    public interface IMessage
    {
        public string GetMessageType();

        public List<VariableValuePair> GetVarValues();
    }

    public struct OKMessage : IMessage
    {
        List<VariableValuePair> varValues;

        public string GetMessageType() => "ok";

        public List<VariableValuePair> GetVarValues() => varValues;
    }

    public struct NoGoodMessage : IMessage
    {
        List<VariableValuePair> varValues;

        public string GetMessageType() => "nogood";

        public List<VariableValuePair> GetVarValues() => varValues;
    }

    private readonly Queue<IMessage> messages = new Queue<IMessage>();

    // Information about variable
    private CSP<T> csp;

    [SerializeField]
    private string ID;

    [SerializeField]
    private int priority;

    [SerializeField]
    private List<string> neighbors = new List<string>();

    private List<VariableValuePair> view = new List<VariableValuePair>();

    private List<List<VariableValuePair>> noGoods = new List<List<VariableValuePair>>();

    public ABTAgent(CSP<T> csp, string varID, int varPriority)
    {
        this.csp = csp;
        ID = varID;
        priority = varPriority;

        // Obtain logical neighbors list (connected by a constraint)
        foreach (var c in this.csp.ConstraintsDictionary[varID])
        {
            foreach (string v in c.variableIDs)
                if (v != varID) neighbors.Add(v);
        }
    }

    public void HandleOK()
    {

    }

    public void HandleNoGood()
    {

    }

    public void CheckView()
    {

    }

    public void Backtrack()
    {

    }
}
