using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GraphLibrary;
using System;

public class GraphVisualizer : MonoBehaviour
{
    [SerializeField]
    private GraphSO graphSO;

    [Header("Visualization")]
    [SerializeField]
    private string seed = "";

    [Header("Nodes")]
    [SerializeField]
    private GameObject nodePrefab;
    [SerializeField]
    private float nodeRadiusMin = .5f;
    [SerializeField]
    private float nodeRadiusMax = .5f;
    // Positioning
    [SerializeField]
    private float minSeparation = 1f;

    [Header("Connections")]
    [Range(0f, 0.1f)]
    public float LineWidth = 0.05f;
    public Material LineMaterial;
    public Gradient LineGradient;


    [SerializeField]
    private bool generate = false;

    // Dynamic Pooling
    private Queue<NodeController> nodePool = new Queue<NodeController>();
    private Queue<ConnectionController> connectionPool = new Queue<ConnectionController>();

    // Nodes currently in use
    public List<NodeController> Nodes = new List<NodeController>();
    public Dictionary<int, int> NodeIndex = new Dictionary<int, int>();
    // Connections currently in use
    public List<ConnectionController> Connections = new List<ConnectionController>();
    public Dictionary<string, int> ConnectionsIndex;

    public Dictionary<int, List<string>> NodeConections 
        = new Dictionary<int, List<string>>();

    // Parent GameObjects
    private Transform graphContainer;
    private Transform nodesContainer;
    private Transform connectionsContainer;

    public DirectedGraph<GenericNode, float> Graph
    {
        get; private set;
    }

    private bool isSetup = false;

    private void OnEnable()
    {
        
    }

    private void OnValidate()
    {
        if (isSetup)
        {
            OnEnable();

            // TODO: Adjust new separation
            foreach (NodeController nc in Nodes)
            {

            }

            foreach (ConnectionController cc in Connections)
            {
                cc.OnValidate();
            }
        }
    }

    private void Update()
    {
        if (generate)
        {
            isSetup = false;
            GenerateAndVisualize();
            isSetup = true;
            generate = false;
        }

        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            OnValidate();
        }
    }

    public void GenerateAndVisualize()
    {
        VisualizeGraph(Generate(graphSO));
    }

    private AbstractGraph<GenericNode, float> Generate(GraphSO graphSO)
    {
        // Generate new graph structure
        var graph = graphSO.Generate();

        Debug.Log("Created Graph with " + graph.VerticesNumber() +
            " nodes and " + graph.EdgesNumber() + " connections");

        return graph;
    }

    private void VisualizeGraph(AbstractGraph<GenericNode, float> graph)
    {
        if (graphContainer == null)
        {
            graphContainer = new GameObject(graphSO.ToString()).transform;
            graphContainer.SetParent(transform);
        }

        VisualizeNodes(graph.GetVertexSet());
        VisualizeConnections(graph.GetEdgeSet());

        isSetup = true;
    }

    private void VisualizeConnections(IEnumerable<IPairValue<GenericNode>> edgeSet)
    {
        // Create connections container
        if (connectionsContainer == null)
        {
            connectionsContainer = new GameObject("Connections").transform;
            connectionsContainer.SetParent(graphContainer);
        }

        // Save all connections in pool and reset list
        foreach (ConnectionController c in Connections)
        {
            PoolConnection(c);
        }
        Connections = new List<ConnectionController>();
        // Stores indices for the 2 connected nodes, can be ordered or not
        ConnectionsIndex = new Dictionary<string, int>();
        NodeConections = new Dictionary<int, List<string>>();

        foreach (IPairValue<GenericNode> edge in edgeSet)
        {
            ConnectionController c = GetConnection();
            if (c == null)
            {
                // Setup connection renderer
                string connectionName =
                    edge.GetFirst().GetName() + "_" +
                    edge.GetSecond().GetName() + "_connection";

                LineRenderer lr = new GameObject(connectionName,
                    typeof(LineRenderer)).GetComponent<LineRenderer>();

                lr.transform.SetParent(connectionsContainer);

                c = lr.gameObject.AddComponent<ConnectionController>();

                // Setup line controller
                c.Setup(
                    this, lr,
                    edge.GetFirst().GetID(),
                    edge.GetSecond().GetID());
            }

            /*Debug.Log("Adding connection for edge "
                + edge.GetFirst().GetName() + " " + edge.GetSecond().GetName() + 
                " ID=" + c.GetID());*/

            // Avoid duplicate connections TODO: unless graph is directed
            string inverseID = edge.GetSecond().GetID() + "_" + edge.GetFirst().GetID();
            if (ConnectionsIndex.ContainsKey(inverseID))
            {
                PoolConnection(c);
                continue;
            }

            ConnectionsIndex.Add(c.GetID(), Connections.Count);
            Connections.Add(c);

            IndexNodeConnections(c);
        }
    }

    /// <summary>
    /// Index conection with all nodes involved
    /// </summary>
    /// <param name="c"></param>
    private void IndexNodeConnections(ConnectionController c)
    {
        if (!NodeConections.ContainsKey(c.N1)) 
            NodeConections.Add(c.N1, new List<string>());

        NodeConections[c.N1].Add(c.GetID());

        if (!graphSO.directed)
        {
            if (!NodeConections.ContainsKey(c.N2))
                NodeConections.Add(c.N2, new List<string>());

            NodeConections[c.N2].Add(c.GetID());
        }
    }

    private void VisualizeNodes(IEnumerable<GenericNode> vertexSet)
    {
        // Create node container if needed
        if (nodesContainer == null)
        {
            nodesContainer = new GameObject("Nodes").transform;
            nodesContainer.SetParent(graphContainer);
        }

        // Save all nodes in pool and reset list
        foreach (NodeController n in Nodes)
        {
            PoolNode(n);
        }
        Nodes = new List<NodeController>();
        NodeIndex = new Dictionary<int, int>();

        // Initialize nodes
        foreach (var vertex in vertexSet)
        {
            NodeController node = GetNode();
            if (node == null) node =
                    Instantiate(nodePrefab, nodesContainer)
                    .GetComponent<NodeController>();

            NodeIndex.Add(vertex.GetID(), Nodes.Count);
            Nodes.Add(node);
        }

        // Positioning
        PackNodes();
    }

    // Pooling TODO: Move to general class

    private void PoolNode(NodeController nc)
    {
        nodePool.Enqueue(nc);
    }

    private NodeController GetNode()
    {
        if (nodePool.Count > 0)
            return nodePool.Dequeue();

        return null;
    }

    private void PoolConnection(ConnectionController c)
    {
        connectionPool.Enqueue(c);
    }

    private ConnectionController GetConnection()
    {
        if (connectionPool.Count > 0)
            return connectionPool.Dequeue();

        return null;
    }

    #region Positioning / Distribution (TODO: Move to own class)

    /// <summary>
    /// Packs nodes in a spherical area around a point 
    /// with min distance between them
    /// https://www.codeproject.com/Articles/42067/D-Circle-Packing-Algorithm-Ported-to-Csharp
    /// </summary>
    private void PackNodes()
    {
        // Origin for now
        double minRadius = nodeRadiusMin;
        double maxRadius = nodeRadiusMax;

        // Position nodes randomly
        System.Random rnd = new System.Random(seed.GetHashCode());
        for (int i = 0; i < Nodes.Count; i++)
        {
            Nodes[i].transform.localPosition =
                new Vector3(
                    (float)(rnd.NextDouble() * minRadius),
                    (float)(rnd.NextDouble() * minRadius),
                    (float)(rnd.NextDouble() * minRadius));

            float radius = (float)(minRadius
                + rnd.NextDouble() * (maxRadius - minRadius));

            Nodes[i].transform.localScale = Vector3.one * radius * 2f;
        }

        int maxIterations = 100;
        StartCoroutine(SeparateNodes(maxIterations));
    }

    // TODO: Implement using Burst and/or GPU instancing
    IEnumerator SeparateNodes(int maxIterations)
    {
        int iterationsCount = 1;
        // TODO: Change dynamically based on framerate
        int maxOperationsPerFrame = 10000;
        int operationsCount = 0;

        // Copy nodes array to maintain original indices
        List<NodeController> nodesCopy = new List<NodeController>(Nodes);

        while (iterationsCount < maxIterations)
        {
            // Sort based on sqr distance to center
            nodesCopy.Sort(Comparer);

            float minSeparationSqr = minSeparation * minSeparation;
            // Check each node with each of the following
            for (int i = 0; i < nodesCopy.Count - 1; i++)
            {
                for (int j = i + 1; j < nodesCopy.Count; j++)
                {
                    if (i == j) continue;

                    Vector3 diff = nodesCopy[j].transform.localPosition
                        - nodesCopy[i].transform.localPosition;

                    float r = nodesCopy[i].Radius + nodesCopy[j].Radius;

                    // Length squared = (dx * dx) + (dy * dy);
                    float d = diff.sqrMagnitude - minSeparationSqr;
                    float minSepSqr = Mathf.Min(d, minSeparationSqr);
                    d -= minSepSqr;

                    if (d < (r * r))
                    {
                        diff.Normalize();
                        diff *= ((r - Mathf.Sqrt(d)) * 0.5f);

                        nodesCopy[j].transform.localPosition += diff;
                        nodesCopy[i].transform.localPosition -= diff;
                    }

                    operationsCount++;
                    if (operationsCount > maxOperationsPerFrame)
                    {
                        operationsCount = 0;
                        OnValidate();
                        yield return null;
                    }
                }
            }

            iterationsCount++;
            //yield return null;
        }

        
    }

    private int Comparer(NodeController n1, NodeController n2)
    {
        float d1 = n1.transform.localPosition.sqrMagnitude;
        float d2 = n2.transform.localPosition.sqrMagnitude;
        if (d1 < d2)
            return 1;
        else if (d1 > d2)
            return -1;
        else
            return 0;
    }

    #endregion
}
