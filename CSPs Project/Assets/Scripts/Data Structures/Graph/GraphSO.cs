using UnityEngine;
using GraphLibrary;
using System.Collections.Generic;


// TODO: Implementar interfaz IGraph por su cuenta para usar un tipo definido

[CreateAssetMenu(fileName = "Graph_", menuName = "Data Structures/Graph")]
[System.Serializable]
// TODO: Custom Data Sctruct Interface with Generate method
public  class GraphSO : ScriptableObject
{
    [Header("Graph Settings")]
    public string seed = "";
    public uint nodesCount = 1;
    public string namePrefix = "X";
    [Range(0f, 1f)]
    public float density = .5f;

    public bool directed = false;

    // TODO: Define contained structure, maybe with other SO (IGraphNode)
    // Should store node prefab

    // TODO: Parametrize if labeled
    // TODO: What to do if directed
    // TODO: Coroutine this somehow
    public DirectedGraph<GenericNode, float> Generate()
    {
        System.Random rng = new System.Random(seed.GetHashCode());

        // Create node pool array
        string[] variableNames = new string[nodesCount];
        GenericNode[] nodes = new GenericNode[nodesCount];

        // Double queue system
        Dictionary<int, List<int>> possibleEdges = new Dictionary<int, List<int>>();
        List<int> keys = new List<int>();
        for (int i = 0; i < nodesCount; i++)
        {
            variableNames[i] = namePrefix + "_" + (i);
            nodes[i] = new GenericNode()
            {
                Name = variableNames[i]
            };

            // Add all possible connections with other nodes
            possibleEdges.Add(i, new List<int>());
            keys.Add(i);
            for (int j = 0; j < nodesCount; j++)
            {
                if (j == i) continue;

                // Avoid two way connections if not directed
                if (!directed
                    && possibleEdges.ContainsKey(j)
                    && possibleEdges[j].Contains(i)) continue;

                possibleEdges[i].Add(j);
            }
        }

        // Create edge pool array
        int edgesCount = 0;
        List<IPairValue<GenericNode>> edges = new List<IPairValue<GenericNode>>();

        Dictionary<int, List<int>> indexedEdges = new Dictionary<int, List<int>>();
        int total = (int)(nodesCount * (nodesCount - 1));
        if (nodesCount > 1)
        {
            int maxIter = 20000;
            int iterCounter = 0;
            
            if (!directed) total /= 2;

            // Connect nodes according to density
            while ((float)edgesCount / total < density)
            {
                //Debug.Log("keys: " + keys.Count);
                int i = keys[rng.Next(0, keys.Count)];

                if (possibleEdges[i].Count == 0)
                {
                    keys.Remove(i);
                    if (keys.Count <= 0) break;
                    continue;
                }

                //Debug.Log("i " + i + " options: " + possibleEdges[i].Count);
                int j = possibleEdges[i][rng.Next(0, possibleEdges[i].Count)];
                //Debug.Log("j " + j);

                GenericNode n1 = nodes[i];
                GenericNode n2 = nodes[j];

                IPairValue<GenericNode> edge =
                    new DirectedPairValue<GenericNode>(n1, n2);

                // Add edge to dictionary
                if (!indexedEdges.ContainsKey(n1.GetID()))
                    indexedEdges.Add(n1.GetID(), new List<int>());

                indexedEdges[n1.GetID()].Add(n2.GetID());
                edges.Add(edge);

                //Debug.Log("Added edge " + edge.GetFirst().Name + "_" + edge.GetSecond().Name);

                // Remove used pairs
                possibleEdges[i].Remove(j);
                //Debug.Log("i " + i + " now has " + possibleEdges[i].Count + " options");
                if (possibleEdges[i].Count == 0)
                {
                    keys.Remove(i);
                }
                if (keys.Count == 0) break;

                edgesCount += 1;

                //if (++iterCounter > maxIter) break;
            }
        }

        Debug.Log("Created " + nodes.Length + " nodes with " 
            + edgesCount + " connections " 
            + "(" + ((float)edgesCount / total) + ")");

        // TODO: Also send edges dictionary to store inside graph
        var graph = new DirectedGraph<GenericNode, float>(nodes, edges.ToArray());

        return graph;
    }

    /*private int GetCloseNotZero(int i)
    {

    }*/
}
