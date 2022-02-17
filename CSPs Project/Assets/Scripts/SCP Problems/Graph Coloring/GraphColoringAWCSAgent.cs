using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Crear clase Agent que guarde una lista dinámica de agentes con los que puede comunicarse.
// Puede inicializarse con esa lista y que no varíe o cambiarla según agentes en su "rango" o detectables por sus sentidos
// Crear estructura abstracta con Senses y Actuators
// Utilizar Corutinas para todos sus métodos (o sync await)
// NO UTILIZAR UN MANAGER

public class GraphColoringAWCSAgent : MonoBehaviour
{
    private GraphColoringCSPVisualizer visualizer;

    private AWCSManager<Color> manager;
    private string agentID;

    private AWCSAgent<Color> agent => manager.GetAgent(agentID);

    // TODO: OnAssignNewValue que llame a Simulation.Stop() o Pause()

    public void Setup(AWCSManager<Color> awcsManager, string agentID)
    {
        manager = awcsManager;
        this.agentID = agentID;
        visualizer = FindObjectOfType<GraphColoringCSPVisualizer>();
    }

    private void Update()
    {
        if (visualizer.IsPaused || agent.Stopped) return;

        if (agent.Messages.Count == 0)
        {
            Debug.Log("<color=red>" + agent.ID + " HAS NO MESSAGES</color>");
        }

        // Handle all received messages
        while (agent.Messages.Count > 0)
        {
            HandleMessage();
        }

        // Check view after all messages are processed
        CheckView();
    }

    private void HandleMessage()
    {
        AWCSMessage<Color> message = 
            (AWCSMessage<Color>)agent.Messages.Dequeue();

        Debug.Log(agent.ID + " handling " + message.Print());

        switch (message.Type)
        {
            case DiSCPAgentMessage<Color>.MessageType.OK:
                // Add to View
                agent.AddToView(message.Contents[0]);
                break;
            case DiSCPAgentMessage<Color>.MessageType.NOGOOD:

                // Add to NoGoods
                agent.NoGoods.Add(message.Contents);

                foreach (var p in message.Contents)
                {
                    // If variable in nogood is not a neighbor and not itself
                    if (!agent.Neighbors.Contains(p.ID) && agent.ID != p.ID)
                    {
                        // Add nogood pair to view
                        agent.View.Add(p);

                        // Add to local neighbors
                        agent.AddNeighbor(p.ID);
                    }
                }
                break;
            case DiSCPAgentMessage<Color>.MessageType.ADDME:
                // Add as new neighbor
                agent.AddNeighbor(message.Contents[0].ID);
                Debug.Log(agent.ID + " added " + message.Contents[0].ID + " as a neighbor.");

                // TODO: Agregar conexiones en el grafo?

                break;
        }
    }

    private void CheckView()
    {
        Debug.Log("<color=cyan>=====" + agent.ID + " Checking view with value " + agent.value + "=====</color>");
        Debug.Log(agent.PrintView());
        // Check consistency with current value and View
        bool consistent = agent.IsViewConsistent();

        if (!consistent)
        {
            // Assign consistent value
            bool assigned = agent.AssignValue(true);

            // If no value is consistent with view
            if (!assigned)
                Backtrack();
            else
            {
                Debug.Log("<color=yellow>" + agent.ID + " view was inconsistent, assigned new value " + agent.value + "</color>");
                agent.SendOK();

                UpdateView();
            }

            agent.SetConsistent(false);
        }
        else
        {
            Debug.Log("<color=cyan>" + agent.ID + " view is consistent!" + "</color>");
            agent.SetConsistent(true);
        }
    }

    private void UpdateView()
    {
        visualizer.Step = true;
    }

    private void Backtrack()
    {
        Debug.Log("<color=yellow>Coulnd't find consistent values in domain, " + agent.ID + " BACKTRACKING</color>");

        bool sentNoGood = agent.SendNoGood();

        if (!sentNoGood)
        {
            Debug.Log("NO SOLUTION!!");
            agent.SendNoSolution();
            return;
        }
    }
}
