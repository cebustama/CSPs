using UnityEngine;
using GraphLibrary;


// TODO: Implementar interfaz IGraph por su cuenta para usar un tipo definido

[CreateAssetMenu(fileName = "Graph_", menuName = "Data Structures/Graph")]
[System.Serializable]
// TODO: Custom Data Sctruct Interface with Generate method
public  class GraphSO : ScriptableObject
{
    [Header("Graph Settings")]
    public string seed = "";
    public uint nodesCount = 1;
    public string nodeName = "X";
    [Range(0f, 1f)]
    public float density = .5f;

    // TODO: Define contained structure, maybe with other SO (IGraphNode)
    // Should store node prefab

    // TODO: Parametrize if labeled, if directed, etc
    public DirectedGraph<GenericNode, float> Generate()
    {
        System.Random rng = new System.Random(seed.GetHashCode());

        // Create node pool array
        string[] variableNames = new string[nodesCount];
        GenericNode[] nodes = new GenericNode[nodesCount];
        for (int i = 0; i < nodesCount; i++)
        {
            variableNames[i] = nodeName + "_" + (i + 1);
            nodes[i] = new GenericNode()
            {
                Name = variableNames[i]
            };
        }

        // Create edge pool array
        int edgesCount = 0;
        IPairValue<GenericNode>[] edges = 
            new IPairValue<GenericNode>[nodesCount * nodesCount];
        if (nodesCount > 1)
        {
            // Connect nodes according to density
            while (edgesCount / (nodesCount * (nodesCount - 1)) < density)
            {
                GenericNode n1 = nodes[rng.Next(0, (int)nodesCount)];
                GenericNode n2 = nodes[rng.Next(0, (int)nodesCount)];

                while (n1.Equals(n2))
                    n2 = nodes[rng.Next(0, (int)nodesCount)];

                // Create two way connection
                IPairValue<GenericNode> edge1 = 
                    new DirectedPairValue<GenericNode>(n1, n2);

                IPairValue<GenericNode> edge2 =
                    new DirectedPairValue<GenericNode>(n2, n1);

                // Add both directions
                edges[edgesCount] = edge1;
                edges[edgesCount + 1] = edge2;

                edgesCount += 2;
            }
        }

        var graph = new DirectedGraph<GenericNode, float>(nodes, edges);

        return graph;
    }
}
