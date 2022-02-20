using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GraphLibrary;

public class GraphVisualizer : MonoBehaviour
{
    [SerializeField]
    private GraphSO graphSO;

    [Header("Visualization")]
    [SerializeField]
    private GameObject nodePrefab;

    [SerializeField]
    private float nodeScale = 1f;

    [SerializeField, Range(0f, 0.1f)]
    private float lineWidth = 0.05f;

    [SerializeField]
    private Material lineMaterial;

    [SerializeField]
    private Gradient lineGradient;


    [SerializeField]
    private bool generate = false;

    // Dynamic Pooling
    private Queue<NodeController> nodePool = new Queue<NodeController>();
    private Queue connectionPool = new Queue();

    private Transform nodesContainer;
    private Transform connectionsContainer;

    public DirectedGraph<GenericNode, float> Graph
    {
        get; private set;
    }

    private void OnEnable()
    {
        
    }

    private void OnValidate()
    {
        OnEnable();
    }

    private void Update()
    {
        if (generate)
        {
            GenerateGraph();
            generate = false;
        }
    }

    private void GenerateGraph()
    {
        Graph = graphSO.Generate();

        // Create node container
        if (nodesContainer == null)
        {
            nodesContainer = new GameObject("Nodes").transform;
            nodesContainer.SetParent(transform);
        }

        // Initialize pools
        foreach (var vertex in Graph.GetVertexSet())
        {
            GameObject node = Instantiate(nodePrefab, nodesContainer);
            nodePool.Enqueue(new NodeController());
        }

        // TODO: Positioning
        // https://www.codeproject.com/Articles/42067/D-Circle-Packing-Algorithm-Ported-to-Csharp

        // Create connections container
        if (connectionsContainer == null)
        {
            connectionsContainer = new GameObject("Connections").transform;
            connectionsContainer.SetParent(transform);
        }



        

        Debug.Log("Created Graph with " + Graph.VerticesNumber() + 
            " nodes and " + Graph.EdgesNumber() + " connections");
    }
}
