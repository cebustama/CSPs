using UnityEngine;

public class GraphColoringABTAgent : MonoBehaviour
{
    [SerializeField]
    private ABTAgent<Color> ABTAgent;

    public void Setup(CSP<Color> csp, string varID, int varPriority)
    {
        ABTAgent = new ABTAgent<Color>(csp, varID, varPriority);
    }
}
