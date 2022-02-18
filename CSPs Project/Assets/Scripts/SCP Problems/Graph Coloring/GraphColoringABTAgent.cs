using UnityEngine;

// Implementation of ABTAgent using Unity MonoBehaviours

// Agent can process multiple messages at the same time
// ProcessView only after processing current messages
public class GraphColoringABTAgent : MonoBehaviour
{
    private GraphColoringCSPVisualizer visualizer;

    private ABTManager<Color> manager;
    private string agentID;

    private ABTAgent<Color> agent => manager.GetAgent(agentID);

    public void Setup(ABTManager<Color> aBTManager, string agentID)
    {
        manager = aBTManager;
        this.agentID = agentID;
        visualizer = FindObjectOfType<GraphColoringCSPVisualizer>();
    }

    private void Update()
    {
        if (visualizer.IsPaused || agent.Stopped) return;

        // Handle all incoming messages
        while (agent.Messages.Count > 0)
        {
            // TODO: PARA QUE SEA REALMENTE PARALELO: Coroutine each handling
            HandleMessage();
        }

        // Check view after all messages are processed
        CheckView();
    }

    private void HandleMessage()
    {
        ABTMessage<Color> message = (ABTMessage<Color>) agent.Messages.Dequeue();
        Debug.Log(agent.Name + " handling " + message.Print());

        switch (message.Type)
        {
            case DiSCPAgentMessage<Color>.MessageType.OK:
                // Add to view or replace current value
                agent.AddToView(message.Contents[0]);
                break;
            case DiSCPAgentMessage<Color>.MessageType.NOGOOD:

                // Add to NoGoods if not already there
                agent.AddNoGood(message.Contents);

                foreach (DiSCPAgentViewTuple<Color> p in message.Contents)
                {
                    // If variable in nogood is not a neighbor and not itself
                    if (!agent.Neighbors.Contains(manager.CSP.GetVariable(p.Name).id) 
                        && agent.Name != p.Name)
                    {
                        // Add nogood pair to view
                        agent.View.Add(p);

                        // Ask to be added to neighbors
                        agent.SendAddMeTo(p.Name);
                    }
                }
                break;
            case DiSCPAgentMessage<Color>.MessageType.ADDME:
                // Add as new neighbor
                agent.AddNeighbor(message.Contents[0].Name);
                Debug.Log(agent.Name + " added " + message.Contents[0].Name + " as a neighbor.");

                // TODO: Agregar conexiones en el grafo?

                break;
        }
    }

    private void CheckView()
    {
        Debug.Log("<color=cyan>===== " + agent.Name 
            + " Checking view" + " =====</color>");

        // Check consistency with current value and View
        bool consistent = agent.IsViewConsistent();

        if (!consistent)
        {
            Debug.Log("<color=red>" + agent.Name + " view is inconsistent." + "</color>");
            // AssignFirst
            bool assigned = agent.AssignFirstConsistent();

            // If no value is consistent with view
            if (!assigned)
                Backtrack();
            else
            {
                Debug.Log("<color=cyan>" + agent.Name + " assigned new value " + agent.value + "</color>");
                agent.SendOK();

                UpdateView();
            }

            agent.SetConsistent(false);
        }
        else
        {
            Debug.Log("<color=green>" + agent.Name + " view is consistent!" + "</color>");
            agent.SetConsistent(true);
        }


        Debug.Log("<color=cyan>=====</color>");
    }

    private void UpdateView()
    {
        visualizer.Step = true;
    }

    private void Backtrack()
    {
        Debug.Log("<color=red>" + "Couldn't find consistent values in domain, " 
            + agent.Name + " BACKTRACKING" + "</color>");
        bool noSolution = !agent.SendNoGood();

        if (noSolution)
        {
            Debug.Log("<color=green>NO SOLUTION!!</color>");
            agent.SendNoSolution();
        }
    }
}
