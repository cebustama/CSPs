using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistributedDFSTreeBuilder : MonoBehaviour
{
    private GraphVisualizer visualizer;

    private Dictionary<int, DistributedTreeNodeAgent> nodeAgents;

    public void Initialize()
    {
        visualizer = GetComponent<GraphVisualizer>();
        nodeAgents = new Dictionary<int, DistributedTreeNodeAgent>();

        // First create every agent
        foreach (int index in visualizer.NodeIndex.Keys)
        {
            CreateAgent(index);
        }

        // Initialize neighbors for each agent
        foreach (DistributedTreeNodeAgent a in nodeAgents.Values)
        {
            InitializeNeighbors(a);
        }

        // Choose root (for now randomly)
        var root = nodeAgents.ElementAt(Random.Range(0, nodeAgents.Count)).Value;
        root.root = true;

        // Start Agents
        foreach (DistributedTreeNodeAgent a in nodeAgents.Values)
        {
            a.firstSetup = true;
        }
    }

    private void CreateAgent(int index)
    {
        NodeController nc = visualizer.Nodes[visualizer.NodeIndex[index]];
        DistributedTreeNodeAgent na =
            nc.gameObject.AddComponent<DistributedTreeNodeAgent>();

        nodeAgents.Add(index, na);

        na.Create(index, nc);
    }

    private void InitializeNeighbors(DistributedTreeNodeAgent a)
    {
        //Debug.Log("Finding Neightbors for node " + a.Index);

        List<DistributedTreeNodeAgent> N = new List<DistributedTreeNodeAgent>();
        List<string> connections = visualizer.NodeConections[a.Index];
        //Debug.Log("Found " + connections.Count + " connections");
        foreach (string c in connections)
        {
            ConnectionController cc = visualizer.Connections[
                visualizer.ConnectionsIndex[c]];

            //Debug.Log("Checking connection " + c);

            int nID = (cc.N1 != a.Index) ? cc.N1 : cc.N2;
            N.Add(nodeAgents[nID]);
        }

        a.AddNeighbors(N);
    }
}
