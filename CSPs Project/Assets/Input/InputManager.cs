using UnityEngine;

using static UnityEngine.InputSystem.InputAction;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private GameObject fpsPanel;

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

        actionsSetup = true;
    }

    private void ToggleFPSCounter(CallbackContext ctx)
    {
        fpsPanel.SetActive(!fpsPanel.activeSelf);
    }
}
