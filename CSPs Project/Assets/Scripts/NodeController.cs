using UnityEngine;

using TMPro;
using UnityEngine.UI;


public class NodeController : MonoBehaviour
{
    // TODO: Generalize this with other types
    public CSP<Color>.CSPVariable<Color> Variable { get; private set; }

    [Header("References")]
    [SerializeField]
    private Transform canvasContainer;

    [SerializeField]
    private Canvas nameCanvas;

    [SerializeField]
    private TextMeshProUGUI variableNameText;

    [SerializeField]
    private Canvas domainCanvas;

    [SerializeField]
    private TextMeshProUGUI domainText;

    // TOOD: UIDomainManager
    [SerializeField]
    private GridLayoutGroup domainLayout;

    // TODO: Create controller class
    [SerializeField]
    private GameObject domainElementPrefab;

    [SerializeField]
    private Canvas informationCanvas;

    [SerializeField]
    private TextMeshProUGUI informationText;

    public Material NodeMaterial { get; private set; }

    public delegate void ValueChange();
    public ValueChange OnValueChange;

    private void Awake()
    {
        NodeMaterial = GetComponent<MeshRenderer>().material;

        SetupUI();
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            canvasContainer.localRotation = Quaternion.Inverse(transform.rotation);
        }
    }

    private void SetupUI()
    {
        nameCanvas.worldCamera = Camera.main;
        domainCanvas.worldCamera = Camera.main;
    }

    public void Connect(CSPGraphColoring gcCSP, CSP<Color>.CSPVariable<Color> var)
    {
        Variable = var;

        variableNameText.SetText(Variable.name);

        // Domain text
        string domain = "D:{";
        foreach (Color c in Variable.domain)
        {
            domain += (c.ToString() + " ");
        }
        domain += "}";
        domainText.SetText(domain);

        // Domain layout
        foreach (Color c in Variable.domain)
        {
            Image i = Instantiate(domainElementPrefab, domainLayout.transform).GetComponent<Image>();
            i.color = c;
        }

        // Information
        var vertex = gcCSP.Graph.GetVertex(var.name);
        informationText.SetText("Degree: " + gcCSP.Graph.Degree(vertex) / 2);
        informationCanvas.gameObject.SetActive(true);
    }

    public void UpdateNode()
    {
        OnValueChange?.Invoke();
    }

    // TODO: Delegates/Events for on hover, on click, on mouse enter/exit
}
