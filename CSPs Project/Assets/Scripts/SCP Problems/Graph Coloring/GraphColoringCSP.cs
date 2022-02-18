using System;
using UnityEngine;

using GraphLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Collections;

/// <summary>
/// Specific type of Constraint Satisfaction Problem about coloring neighbouring graphs
/// </summary>
[Serializable]
// TODO: Create CSPGraph class, CSPs which receive graphs as input
public class GraphColoringCSP : CSP<Color>
{
    [Serializable]
    public class GraphNode : IVertex
    {
        public string Name;

        public string GetID() => Name;
    }

    public UndirectedGraph<GraphNode, int> Graph { get; private set; }

    public GraphColoringCSP(string[] names, Color[][] domains, float density, string seed) : base(names, domains, Color.black)
    {
        // TODO: Move this outside, receive graph and construct Variables/Constraints Data
        GenerateGraph(names, density, seed);

        // Setup constraints here based on graph neighbors
        foreach (var s in Graph.GetEdgeSet())
        {
            AddPairConstraint(s.GetFirst().Name, s.GetSecond().Name);
        }
    }

    private void GenerateGraph(string[] names, float density, string seed)
    {
        System.Random rng = new System.Random(seed.GetHashCode());

        Graph = new UndirectedGraph<GraphNode, int>();

        // Create vertices
        for (int i = 0; i < names.Length; i++)
        {
            Graph.AddVertex(new GraphNode()
            {
                Name = names[i]
            });
        }

        // Connect vertices according to density
        float vertices = Graph.VerticesNumber();
        List<GraphNode> vertexList = Graph.GetVertexList();
        while ((Graph.EdgesNumber() / (vertices * (vertices - 1)) < density))
        {
            GraphNode n1 = vertexList[rng.Next(0, vertexList.Count)];
            GraphNode n2 = Graph.GetVertexOtherThan(n1, rng);

            // Add both directions
            Graph.AddEdge(n1, n2, 0);
            Graph.AddEdge(n2, n1, 0);
        }
    }

    // TODO: Generalize these two?
    private void AddPairConstraint(string v1, string v2)
    {
        string[] vars = new string[] { v1, v2 };
        AddConstraint(vars, CheckNotEqual);
    }

    public BinaryConstraint<Color> GetPairConstraint(string v1, string v2)
    {
        foreach (BinaryConstraint<Color> c in Constraints)
        {
            if (c.v1 == v1 && c.v2 == v2) return c;
        }

        return null;
    }

    // Only add binary constraints in this problem
    protected override void AddConstraint(string[] variables, Func<Color[], bool> condition)
    {
        BinaryConstraint<Color> dirC = new BinaryConstraint<Color>(variables, condition);
        Constraints.Add(dirC);
        IndexConstraint(variables, dirC);
    }

    // TODO: Where to put these, Generalize?
    private Func<Color[], bool> CheckNotEqual = (Color[] values) =>
    {
        Color first = values[0];
        foreach (Color c in values)
        {
            if (!Equals(first, c))
                return true;
        }

        return false;
    };

    // Solve problem
    public override void Solve()
    {
        UnityEngine.Debug.Log("<color=red>Solving Graph Coloring CSP</color>");
        // TODO: CSP.Print()
        //SolveGreedy(this);

        var watch = Stopwatch.StartNew();
        SolveBT(this);
        watch.Stop();
        UnityEngine.Debug.Log(watch.ElapsedMilliseconds + "[ms]");
    }

    public override void SolveWithAgents(GameObject[] agents, string seed)
    {
        UnityEngine.Debug.Log("<color=red>Solving Graph Coloring CSP</color>");
        var watch = Stopwatch.StartNew();


        // TODO: Decide here wheter to send this csp or a new copy

        //SolveABT(this, agents, watch, seed);
        SolveAWCS(this, agents, seed);
    }

    // TODO: Create visualizer class
    public void SolveStep(GraphColoringCSPVisualizer visualizer)
    {
    }

    // TODO: Cómo implementar esto de manera modular y general
    #region Solving Algorithms

    // Adds colors when needed
    // Creates a solution
    private void SolveGreedy(CSP<Color> csp)
    {
        UnityEngine.Debug.Log("<color=green>Starting Greedy Algorithm</color>");

        double t0 = Time.time;

        // TODO: Generalize ordering mehods
        List<Variable<Color>> orderedVariables = OrderVariables(vars =>
        {
            // Randomize
            return vars.OrderBy(a => rng.Next()).ToList();
        });

        List<string> assignedVars = new List<string>();

        int addedValues = 0;
        while (assignedVars.Count < orderedVariables.Count)
        {
            string varID = VariableNames[orderedVariables[assignedVars.Count].id];

            bool foundInDomain = csp.AssignFirstValid(varID, true, () =>
            {
                return UnityEngine.Random.ColorHSV();
            });

            if (!foundInDomain) addedValues++;

            /*if (csp.VariablesDictionary[varID].ValueInDomain())
                UnityEngine.Debug.Log("Assigned " + csp.VariablesDictionary[varID] + " to " + varID);
            else
                UnityEngine.Debug.Log("Added new value to " + varID);*/

            assignedVars.Add(varID);
        }

        UnityEngine.Debug.Log("Soluion found in " + (Time.time - t0) + "[ms]. Added " + addedValues + " new values not in domains.");
    }

    private void SolveBT(CSP<Color> csp, GraphColoringCSPVisualizer visualizer = null)
    {
        UnityEngine.Debug.Log("<color=green>Starting Backtracking Algorithm</color>");

        List<Variable<Color>> orderedVariables = OrderVariables(vars =>
        {
            return vars.OrderByDescending(a => 
                Graph.Degree(
                    Graph.GetVertex(
                        VariableNames[a.id]
                ))).ToList();
        });

        int maxIterations = 1000;

        bool found = SelectValue(csp, orderedVariables, 0, maxIterations, 1);

        if (found)
            UnityEngine.Debug.Log("Solution found using " + csp.GetDifferentValues().Count + " colors");
        else
            UnityEngine.Debug.Log("No solution found");
    }

    // Recursive Function, modifies CSP directly
    // TODO: Get solution but don't modify CSP instance
    private bool SelectValue(
        CSP<Color> csp, List<Variable<Color>> orderedVariables,
        int varIndex, int maxIterations, int iterationNumber, bool changeValue = false)
    {
        if (iterationNumber >= maxIterations)
            return false;

        bool foundValue = false;

        if (!changeValue)
        {
            UnityEngine.Debug.Log("Selecting value for " + VariableNames[orderedVariables[varIndex].id]
                + " (assigned: " + (varIndex + 1) + "/" + orderedVariables.Count
                + " iteration " + iterationNumber + ")");

            foundValue = csp.AssignFirstValid(VariableNames[orderedVariables[varIndex].id]);
        }
        else
        {
            UnityEngine.Debug.Log("<color=red>BACKTRACK</color>: Selecting value for " + VariableNames[orderedVariables[varIndex].id]
                + " (assigned: " + (varIndex + 1) + "/" + orderedVariables.Count
                + " iteration " + iterationNumber + ")");

            foundValue = csp.AssignValidOtherThan(VariableNames[orderedVariables[varIndex].id], orderedVariables[varIndex].value);
        }

        // Value found, advance
        if (foundValue)
        {
            // Move to next
            if (varIndex < orderedVariables.Count - 1)
            {
                return SelectValue(csp, orderedVariables, ++varIndex, maxIterations, ++iterationNumber);
            }
            // Finish
            else
            {
                return true;
            }
        }
        // No value found, backtrack
        else
        {
            if (varIndex > 0)
                return SelectValue(csp, orderedVariables, --varIndex, maxIterations, ++iterationNumber, true);
            // No solution found
            else
                return false;
        }
    }

    /// <summary>
    /// Asynchronous Backtracking using Node Variable objects as agents
    /// </summary>
    /// <param name="csp"></param>
    /// <param name="agentObjects"></param>
    /// <param name="watch"></param>
    public void SolveABT(CSP<Color> csp, GameObject[] agentObjects, string seed)
    {
        UnityEngine.Debug.Log("Starting ABT with " + csp + " agents:" + agentObjects.Length + " seed:" + seed);
        
        // TODO: TODA ESTA PARTE, hay que ordenar las variables y luego los agentes, o al mismo tiempo de alguna forma
        // Order variables
        List<Variable<Color>> orderedVariables = OrderVariables(vars =>
        {
            //return vars.OrderByDescending(a => Graph.Degree(Graph.GetVertex(a.name))).ToList();
            return vars;
        });

        // Order agentObjects according to variable ids
        List<GameObject> orderedAgents = agentObjects.ToList();
        //orderedAgents = orderedAgents.OrderBy(a =>  )
        //////////////////////////////

        // Create ABT manager
        ABTManager<Color> manager = new ABTManager<Color>(csp);
        
        for (int i = 0; i < orderedVariables.Count; i++)
        {
            UnityEngine.Debug.Log("Creating agent for variable id" + orderedVariables[i].id + " name " + VariableNames[orderedVariables[i].id]);
            // Create Agent
            var ABTAgent = manager.AddAgent(VariableNames[orderedVariables[i].id], orderedVariables.Count - i);

            // Add component to gameobject and setup
            GraphColoringABTAgent agentBehaviour = orderedAgents[i].AddComponent<GraphColoringABTAgent>();
            agentBehaviour.Setup(manager, ABTAgent.Name);

            UnityEngine.Debug.Log("Created agent " + ABTAgent.Name);
        }

        manager.Start(seed);
    }

    public void SolveAWCS(CSP<Color> csp, GameObject[] agentObjects, string seed)
    {
        // Order variables
        List<Variable<Color>> orderedVariables = OrderVariables(vars =>
        {
            //return vars.OrderByDescending(a => Graph.Degree(Graph.GetVertex(a.name))).ToList();
            return vars;
        });

        // Order agentObjects according to variable ids
        List<GameObject> orderedAgents = agentObjects.ToList();

        // Create ABT manager
        AWCSManager<Color> manager = new AWCSManager<Color>(csp);
        for (int i = 0; i < orderedVariables.Count; i++)
        {
            // Create Agent
            var AWCSAgent = manager.AddAgent(VariableNames[orderedVariables[i].id], orderedVariables.Count - i);

            // Add component to gameobject and setup
            GraphColoringAWCSAgent agentBehaviour = orderedAgents[i].AddComponent<GraphColoringAWCSAgent>();
            agentBehaviour.Setup(manager, AWCSAgent.Name);
        }

        manager.Start(seed);
    }

    private IEnumerator WaitForSolution(ABTManager<Color> manager, Stopwatch watch)
    {
        while (!manager.Stopped)
        {
            yield return null;
        }

        watch.Stop();

        if (manager.FoundSolution)
            UnityEngine.Debug.Log("Solution found in " + watch.ElapsedMilliseconds + "[ms]");
        else
            UnityEngine.Debug.Log("No Solution in " + watch.ElapsedMilliseconds + "[ms]");
    }
    #endregion
}
