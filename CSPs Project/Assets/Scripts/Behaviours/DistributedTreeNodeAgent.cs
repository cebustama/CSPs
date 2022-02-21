using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistributedTreeNodeAgent : MonoBehaviour
{
    public int Index { get; private set; }

    public NodeController nodeController;

    // Children
    public List<DistributedTreeNodeAgent> C;
    // Parents
    public List<DistributedTreeNodeAgent> P;
    // Pseudo-children
    public List<DistributedTreeNodeAgent> PC;
    // Pseudp-parents (note: cant be a parent)
    public List<DistributedTreeNodeAgent> PP;

    // All ancestors connected to node or descendants of node
    public List<DistributedTreeNodeAgent> Sep;

    // TODO: Dictionary, mark each neighbor as a type, dont use different lists
    private List<DistributedTreeNodeAgent> N;

    public class DFSToken
    {
        public DistributedTreeNodeAgent remitent;
        public List<DistributedTreeNodeAgent> contents;
    }

    Dictionary<int, DFSToken> receivedTokens = new Dictionary<int, DFSToken>();

    private Dictionary<int, bool> visited;

    public bool root = false;
    public bool firstSetup = false;
    public bool passingToken = false;
    public bool handlingToken = false;

    public bool firstMessage = true;

    private DFSToken currentToken;

    public void Create(int id, NodeController nc)
    {
        Index = id;

        nodeController = nc;

        C = new List<DistributedTreeNodeAgent>();
        P = new List<DistributedTreeNodeAgent>();
        PC = new List<DistributedTreeNodeAgent>();
        PP = new List<DistributedTreeNodeAgent>();
        Sep = new List<DistributedTreeNodeAgent>();
    }

    public void AddNeighbors(List<DistributedTreeNodeAgent> n)
    {
        visited = new Dictionary<int, bool>();
        N = n;

        // Set all neighbors as not visited
        foreach (var a in N)
        {
            visited[a.Index] = false;
        }
    }

    private void Update()
    {
        if (firstSetup)
        {
            if (!passingToken)
                StartCoroutine(PassToken());
        }
    }

    private void SendToken(DistributedTreeNodeAgent recipient, DFSToken token)
    {
        Debug.Log(PrintIndex() + " sending token to " + recipient.PrintIndex());
        recipient.ReceiveToken(token);
    }

    public void ReceiveToken(DFSToken token)
    {
        receivedTokens.Add(token.remitent.Index, token);
    }

    private IEnumerator PassToken()
    {
        passingToken = true;

        // If root, propagate token, else wait
        if (root)
        {
            Debug.Log("<color=red>ROOT </color>" + PrintIndex() + " N=" + N.Count);
            nodeController.NodeMaterial.color = Color.red;
            P = null;

            // Create empty DFS token
            currentToken = new DFSToken()
            {
                remitent = this,
                contents = new List<DistributedTreeNodeAgent>() // Empty list
            };
        }
        else
        {
            // Wait for token
            StartCoroutine(HandleIncomingToken());
            while (handlingToken)
            {
                yield return null;
            }
        }

        // Add itself to DFS token
        currentToken.contents.Add(this);

        // OPTIONAL: ORDER NEIGHBORS

        Debug.Log("<color=blue>" + PrintIndex() + " N=" + N.Count + "</color>");

        // Pass token to UNVISITED neighbors
        foreach (DistributedTreeNodeAgent n in N)
        {
            // If not visited yet
            if (!visited[n.Index]) 
            {
                // Add to children
                C.Add(n);

                // Send token
                currentToken.remitent = this;
                SendToken(n, currentToken);

                // Wait for token back
                StartCoroutine(HandleIncomingToken());
                while (handlingToken)
                {
                    yield return null;
                }

                Debug.Log("<color=green>ADVANCE</color>");
            }
        }

        // TODO: Delete itself from token, return to parent
        Debug.Log("<color=green>HOLI</color>");
    }

    private IEnumerator HandleIncomingToken()
    {
        handlingToken = true;

        // Wait for any message
        while (receivedTokens.Count == 0)
        {
            yield return null;
        }

        // Get awaited token
        DFSToken dfstoken;
        
        // Wait for any
        var pair = receivedTokens.First();
        dfstoken = pair.Value;
        receivedTokens.Remove(pair.Key);
        Debug.Log(PrintIndex() + " received token from " + dfstoken.remitent.PrintIndex());

        // Save remitent as visited
        visited[dfstoken.remitent.Index] = true;

        currentToken = dfstoken;

        if (firstMessage && !root)
        {
            Debug.Log(PrintIndex() + " set " + dfstoken.remitent.PrintIndex() + " as parent");

            nodeController.NodeMaterial.color = Color.green;

            // Add as parent
            P.Add(dfstoken.remitent);

            // Every neighbor CONTAINED IN RECEIVED TOKEN added as pseudoparent
            foreach (var t in dfstoken.contents)
            {
                if (N.Contains(t))
                {
                    Debug.Log(PrintIndex() + " set " + t.PrintIndex() + " as pseudoparent");
                    PP.Add(t);
                }
            }
            firstMessage = false;
        }
        else
        {
            // OPTIONAL: Ordenar vecinos no visitados según heurística
            // If sender is not child add to pseudochildren
            if (!C.Contains(dfstoken.remitent))
            {
                PC.Add(dfstoken.remitent);
            }
        }

        handlingToken = false;
    }

    private string PrintIndex()
    {
        if (!root) return Index.ToString();
        else return "<color=red>" + Index.ToString() + "</color>";
    }
}
