using UnityEngine;

using static UnityEngine.InputSystem.InputAction;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private GameObject fpsPanel;

    [SerializeField]
    private GraphColoringCSPVisualizer visualizer;

    [Header("Settings")]
    [SerializeField]
    private float zoomSpeed = 1f;

    private InputActions actions;

    private bool actionsSetup;

    private void OnEnable()
    {
        if (actionsSetup)
            actions.Default.Enable();
    }

    private void OnDisable()
    {
        if (actionsSetup)
            actions.Default.Disable();
    }

    private void Awake()
    {
        actions = new InputActions();
        actions.Default.FPS.performed += ToggleFPSCounter;
        actions.Default.Step.performed += PerformStep;
        //actions.Default.Zoom.performed += HandleZoom;

        actionsSetup = true;
    }

    private void ToggleFPSCounter(CallbackContext ctx)
    {
        fpsPanel.SetActive(!fpsPanel.activeSelf);
    }

    private void PerformStep(CallbackContext ctx)
    {
        visualizer.IsPaused = false;
    }

    private void HandleZoom(CallbackContext ctx)
    {
        float z = ctx.ReadValue<float>();
        
        if (Camera.main.orthographic)
        {
            if (z > 0)
                Camera.main.orthographicSize -= zoomSpeed;
            else if (z < 0)
                Camera.main.orthographicSize += zoomSpeed;
        }
        else
        {
            if (z > 0)
                Camera.main.transform.position += Vector3.forward * zoomSpeed;
            else if (z < 0)
                Camera.main.transform.position -= Vector3.forward * zoomSpeed;
        }
    }
}
