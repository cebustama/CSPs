using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Implementation of ABTAgent using Unity MonoBehaviours

// Agent can process multiple messages at the same time
// ProcessView only after processing current messages
public class GraphColoringABTAgent : MonoBehaviour
{
    private GraphColoringCSPVisualizer visualizer;

    public ABTAgent<Color> ABTAgent { get; private set; }

    public void Setup(ABTAgent<Color> agent)
    {
        ABTAgent = agent;
        visualizer = FindObjectOfType<GraphColoringCSPVisualizer>();
    }

    private void Update()
    {
        if (visualizer.IsPaused || ABTAgent.Stopped) return;

        // Handle all incoming messages
        while (ABTAgent.Messages.Count > 0)
        {
            // TODO: Coroutine each handling
            HandleMessage();
        }

        // Check view after all messages are processed
        CheckView();
    }

    private void HandleMessage()
    {
        ABTAgent<Color>.ABTMessage message = ABTAgent.Messages.Dequeue();
        Debug.Log(ABTAgent.ID + " handling " + message.Print());

        switch (message.Type)
        {
            case ABTAgent<Color>.ABTMessage.MessageType.OK:
                // Add to View
                ABTAgent.View.Add(message.VarValues[0]);
                break;
            case ABTAgent<Color>.ABTMessage.MessageType.NOGOOD:

                // Add to NoGoods
                ABTAgent.NoGoods.Add(message.VarValues);

                foreach (var p in message.VarValues)
                {
                    // If variable in nogood is not a neighbor, don't add itself
                    if (!ABTAgent.Neighbors.Contains(p.varID) && ABTAgent.ID != p.varID)
                    {
                        // Add nogood pair to view
                        ABTAgent.View.Add(p);

                        // Ask to be added to neighbors
                        ABTAgent.SendAddMeTo(p.varID);
                    }
                }
                break;
            case ABTAgent<Color>.ABTMessage.MessageType.ADDME:
                // Add as new neighbor
                ABTAgent.AddNeighbor(message.VarValues[0].varID);
                Debug.Log(ABTAgent.ID + " added " + message.VarValues[0].varID + " as a neighbor.");

                // TODO: Agregar conexiones en el grafo?

                break;
        }
    }

    private void CheckView()
    {
        Debug.Log(ABTAgent.ID + " Checking view");
        // Check consistency with current value and View
        bool consistent = ABTAgent.IsViewConsistent();

        if (!consistent)
        {
            // AssignFirst
            bool assigned = ABTAgent.AssignFirstConsistent();

            // If no value is consistent with view
            if (!assigned)
            {
                Backtrack();
            }
            else
            {
                Debug.Log(ABTAgent.ID + " view was inconsistent, assigned new value.");
                ABTAgent.SendOK();

                UpdateView();
            }

            ABTAgent.SetConsistent(false);
        }
        else
        {
            Debug.Log(ABTAgent.ID + " view is consistent!");
            ABTAgent.SetConsistent(true);
        }
    }

    private void UpdateView()
    {
        visualizer.Step = true;
    }

    private void Backtrack()
    {
        Debug.Log("Coulnd't find consistent values in domain, " + ABTAgent.ID + " BACKTRACKING");
        bool noSolution = !ABTAgent.SendNoGood();

        if (noSolution)
        {
            Debug.Log("NO SOLUTION!!");
            ABTAgent.SendNoSolution();
        }
    }
}
