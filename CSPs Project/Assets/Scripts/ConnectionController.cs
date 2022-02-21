using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionController : MonoBehaviour
{
    private GraphVisualizer visualizer;
    private LineRenderer lr;
    public int N1, N2; // get private set

    private bool isSetup = false;

    public void Setup(GraphVisualizer v, LineRenderer lr, int n1, int n2)
    {
        visualizer = v;

        this.N1 = n1;
        this.N2 = n2;

        this.lr = lr;

        isSetup = true;

        UpdateLine();
    }

    public void OnValidate()
    {
        if (isSetup)
            UpdateLine();
    }

    private void UpdateLine()
    {
        // Get position count from gradient keys
        GradientColorKey[] keys = visualizer.LineGradient.colorKeys;
        lr.positionCount = keys.Length;

        Transform t1 = visualizer.Nodes[visualizer.NodeIndex[N1]].transform;
        Transform t2 = visualizer.Nodes[visualizer.NodeIndex[N2]].transform;

        // Start position
        lr.SetPosition(0, t1.position);
        // Intermediate points to use gradient
        int positionsLeft = keys.Length - 2;
        if (positionsLeft > 0)
        {
            Vector3 diff = t2.position - t1.position;
            Vector3 offset = diff / (positionsLeft + 1);
            for (int k = 1; k <= positionsLeft; k++)
            {
                lr.SetPosition(k, t1.position + offset * k);
            }
        }
        // End position
        lr.SetPosition(keys.Length - 1, t2.position);

        lr.startWidth = visualizer.LineWidth;
        lr.endWidth = visualizer.LineWidth;
        lr.material = visualizer.LineMaterial;

        lr.colorGradient = visualizer.LineGradient;
    }

    public string GetID()
    {
        return N1.ToString() + "_" + N2.ToString();
    }

    public string GetInverseID()
    {
        return N1.ToString() + "_" + N2.ToString();
    }
}
