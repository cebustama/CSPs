using System;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;

using TMPro;

// Visualizes CSP logic graph where connections = constraints between variables
// TOOD: Rename to GraphColoringController, move all rendering logic to GraphColoringRenderer
public class GraphColoringCSPVisualizer : MonoBehaviour
{
    // TODO: Scriptable Object
    [Header("Graph")]

    [SerializeField]
    private string graphSeed;

    [SerializeField]
    private int numNodes;

    [SerializeField, Range(0f, 1f)]
    private float density = .5f;

    [SerializeField]
    private uint numColors;

    [SerializeField]
    private bool generate;

    // TODO: Configurable node number
    // TODO: Different Domains

    private GraphColoringCSP gcCSP;

    [Header("Visualization")]
    [SerializeField]
    private GameObject nodePrefab;

    [SerializeField]
    private float radius = 1f;

    [SerializeField]
    private float nodeScale = 1f;

    [SerializeField, Range(0f, 0.1f)]
    private float lineWidth = 0.05f;

    [SerializeField]
    private Material lineMaterial;

    [SerializeField]
    private Gradient lineGradient;

    [SerializeField, Range(0, 360)]
    private int initialAngleOffset = 0;

    [SerializeField]
    private TextMeshProUGUI colorsText;

    public enum Ordering
    {
        None,
        Random,
        DegreeDesc
    }

    public enum Algorithm
    {
        Greedy,
        AssignValueBT,
        ABT,
        AWCS
    }

    [Header("Solving")]

    [SerializeField]
    private Algorithm algorithm;

    [SerializeField]
    private Ordering ordering;

    // TODO: Button, add button creation to Islands Engine
    [SerializeField]
    private bool solve;

    public bool Step;
    public bool IsPaused;

    // TODO IDEA use generated sphere or GPU sphere
    // Dictionary stores nodes ids in nodearray (sprites)
    private Dictionary<string, int> nodeIndices;
    private NodeController[] nodes;

    // Connection stores reference to both nodes and it's line renderer object
    // TODO: Use object pooling for LineRenderers (and sprites), store renderer id in connection class/struct
    public class Connection
    {
        public string v1, v2;
        public LineRenderer lr;

        public string GetID()
        {
            return v1 + v2;
        }
    }
    private List<Connection> connections;
    private Dictionary<string, int> connectionsIndices;

    private Transform nodesContainer;
    private Transform connectionsContainer;
    private bool setupDone;

    private int currentNodeNum 
    { 
        get { return (setupDone) ? nodes.Length : 0; } 
    }

    private int currentConnectionNum
    {
        get { return (setupDone) ? connections.Count : 0; }
    }

    private float angleOffset => radians(360f / currentNodeNum);

    private void OnEnable()
    {
        // TODO
    }

    private void OnDisable()
    {
        // TODO
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            OnEnable();
            OnDisable();

            UpdateVisualization();
        }
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            OnValidate();
        }

        if (solve)
        {
            solve = false;

            Debug.Log("csp: " + gcCSP);

            // TODO: Algorithm classes, "default" and distributed (uses agents)
            if (algorithm != Algorithm.ABT && algorithm != Algorithm.AWCS)
                gcCSP.Solve();
            else
            {
                // Use node GameObjects as Agents
                GameObject[] agents = new GameObject[nodes.Length];
                for (int i = 0; i < agents.Length; i++)
                {
                    agents[i] = nodes[i].gameObject;
                }

                Debug.Log("agents " + agents.Length);

                if (algorithm == Algorithm.ABT)
                    gcCSP.SolveABT(gcCSP, agents, graphSeed);
                else if (algorithm == Algorithm.AWCS)
                    gcCSP.SolveAWCS(gcCSP, agents, graphSeed);
            }

            OnValidate();
        }

        // TODO: Call OnValidate each t seconds or f frames
        if (Step)
        {
            Step = false;
            //Debug.Log("<color=blue>PAUSED</color>");
            //IsPaused = true;
            OnValidate();
        }
        
    }

    private void Awake()
    {
        setupDone = false;
        Setup();
        UpdateVisualization();
    }

    private void Setup()
    {
        SetupData();
        // TODO: Revisar esta parte en el profiler, ver si se puede paralelizar de algún modo
        SetupRender();
        setupDone = true;
    }

    private void SetupData()
    {
        string[] variables = new string[numNodes];
        for (int i = 0; i < numNodes; i++)
        {
            variables[i] = "X" + (i + 1);
        }

        // Create color domain
        List<Color> colorOptions = new List<Color>() 
        {
            Color.blue,
            Color.clear,
            Color.cyan,
            Color.green,
            Color.magenta,
            Color.red,
            Color.white,
            Color.yellow,
            new Color(255f / 255f, 165f / 255f, 0f / 255f, 255f / 255f) 
        };

        List<Color> colorsDomain = new List<Color>();
        for (int i = 0; i < numColors; i++)
        {
            Color c = UnityEngine.Random.ColorHSV();
            
            if (colorOptions.Count > 0)
                c = colorOptions[UnityEngine.Random.Range(0, colorOptions.Count)];

            while (colorsDomain.Contains(c))
                c = UnityEngine.Random.ColorHSV();

            colorsDomain.Add(c);
            colorOptions.Remove(c);
        }

        // TODO: Different domains (randomize subset?)
        Color[][] domains = new Color[variables.Length][];
        for (int i = 0; i < variables.Length; i++)
        {
            domains[i] = colorsDomain.ToArray();
        }

        // TODO: Create graph from SO and pass it to CSP

        // Graph coloring 
        gcCSP = new GraphColoringCSP(
            variables, domains, density, graphSeed
        );

        gcCSP.SetNumColors(numColors);
    }

    // TODO: Move all this to other class
    #region Rendering

    /// <summary>
    /// Create Sprite Renderers and position them in space
    /// </summary>
    private void SetupRender()
    {
        nodeIndices = new Dictionary<string, int>();
        nodes = new NodeController[gcCSP.Variables.Length];

        // Nodes (variables)
        nodesContainer = new GameObject("Nodes").transform;
        nodesContainer.SetParent(transform);
        for (int i = 0; i < nodes.Length; i++)
        {
            string varName = gcCSP.VariableNames[gcCSP.Variables[i].id];

            // Create node
            NodeController node = Instantiate(nodePrefab).GetComponent<NodeController>();

            // Connect node to CSP Variable
            node.Connect(gcCSP, gcCSP.Variables[i]);

            // Setup Update delegate (how to handle value change)
            node.OnValueChange += () =>
            {
                node.NodeMaterial.color = node.Variable.value;
            };

            node.transform.SetParent(nodesContainer);

            nodes[i] = node;
            nodeIndices.Add(varName, i);
        }

        AssignMatchingColors();

        // Connections (constraints)
        connectionsContainer = new GameObject("Connections").transform;
        connectionsContainer.SetParent(transform);

        connectionsIndices = new Dictionary<string, int>();
        connections = new List<Connection>();
        for (int i = 0; i < gcCSP.Constraints.Count; i++)
        {
            var c = (CSP<Color>.BinaryConstraint<Color>) gcCSP.Constraints[i];

            // Avoid duplicate connections
            string inverseConnectionID = c.v2 + c.v1;
            if (connectionsIndices.ContainsKey(inverseConnectionID)) continue;

            string connectionName = c.v1 + "_" + c.v2 + "_connection";
            LineRenderer lr = new GameObject(connectionName, typeof(LineRenderer)).GetComponent<LineRenderer>();
            lr.transform.SetParent(connectionsContainer);

            Connection connection = new Connection()
            {
                v1 = c.v1,
                v2 = c.v2,
                lr = lr
            };

            connections.Add(connection);

            connectionsIndices.Add(connection.GetID(), i);
        }
    }

    private void AssignMatchingColors()
    {
        foreach (var n in nodes)
        {
            //n.NodeMaterial.color = lineGradient.Evaluate(UnityEngine.Random.Range(0f, 1f));
        }
    }

    // Returns wolrd position of node with id
    private Vector3 GetNodePosition(string vID)
    {
        if (nodeIndices.ContainsKey(vID))
            return nodes[nodeIndices[vID]].transform.position;

        throw new ArgumentException();
    }

    private void UpdateVisualization()
    {
        for (int i = 0; i < currentNodeNum; i++)
        {
            UpdateNode(i);
        }

        for (int i = 0; i < currentConnectionNum; i++)
        {
            UpdateConnection(i);
        }

        UpdateUI();
    }

    private void UpdateNode(int i)
    {
        // Color
        nodes[i].UpdateNode();

        // Position: Polar coordinates
        nodes[i].transform.localScale = Vector3.one * nodeScale;
        float x = radius * cos(radians(initialAngleOffset) + angleOffset * i);
        float y = radius * sin(radians(initialAngleOffset) + angleOffset * i);
        nodes[i].transform.localPosition = new Vector2(x, y);
    }

    // TODO: Move this to LineRenderer Helper class
    private void UpdateConnection(int i)
    {
        Vector3 v1 = GetNodePosition(connections[i].v1);
        Vector3 v2 = GetNodePosition(connections[i].v2);

        // Get position count from gradient keys
        GradientColorKey[] keys = lineGradient.colorKeys;
        connections[i].lr.positionCount = keys.Length;
        // Start position
        connections[i].lr.SetPosition(0, v1);
        // Intermediate points to use gradient
        int positionsLeft = keys.Length - 2;
        if (positionsLeft > 0)
        {
            Vector3 diff = v2 - v1;
            Vector3 offset = diff / (positionsLeft + 1);
            for (int k = 1; k <= positionsLeft; k++)
            {
                connections[i].lr.SetPosition(k, v1 + offset * k);
            }
        }
        // End position
        connections[i].lr.SetPosition(keys.Length - 1, v2);

        connections[i].lr.startWidth = lineWidth;
        connections[i].lr.endWidth = lineWidth;
        connections[i].lr.material = lineMaterial;

        connections[i].lr.colorGradient = lineGradient;
    }

    private void UpdateUI()
    {
        if (gcCSP == null) return;

        string info =
            "Num Nodes: " + gcCSP.Variables.Length + "\n"
            + "Num Colors: " + gcCSP.GetDifferentValues().Count.ToString();

        colorsText.SetText(info);
    }

    #endregion
}
