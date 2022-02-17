// GENERATED AUTOMATICALLY FROM 'Assets/Input/Input Actions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @InputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @InputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Input Actions"",
    ""maps"": [
        {
            ""name"": ""Default"",
            ""id"": ""0d14afe5-3492-4653-9113-1327516ee41d"",
            ""actions"": [
                {
                    ""name"": ""FPS"",
                    ""type"": ""Button"",
                    ""id"": ""183d161f-35f7-4b38-b75a-5ebc4f98ae6f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Step"",
                    ""type"": ""Button"",
                    ""id"": ""b5c7a484-c3eb-4b9a-840b-e6002861860f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""PassThrough"",
                    ""id"": ""aaf4c300-e9d4-444a-962a-8dba1ed1a5d6"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""3c7bd131-730a-4e06-941c-9d3664140f4d"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""FPS"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4f25e674-cfd3-4505-b266-23ee2d4edadb"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Step"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""73a5f66c-104f-4ac0-9cd0-34f299e616ab"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard + Mouse"",
            ""bindingGroup"": ""Keyboard + Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Default
        m_Default = asset.FindActionMap("Default", throwIfNotFound: true);
        m_Default_FPS = m_Default.FindAction("FPS", throwIfNotFound: true);
        m_Default_Step = m_Default.FindAction("Step", throwIfNotFound: true);
        m_Default_Zoom = m_Default.FindAction("Zoom", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Default
    private readonly InputActionMap m_Default;
    private IDefaultActions m_DefaultActionsCallbackInterface;
    private readonly InputAction m_Default_FPS;
    private readonly InputAction m_Default_Step;
    private readonly InputAction m_Default_Zoom;
    public struct DefaultActions
    {
        private @InputActions m_Wrapper;
        public DefaultActions(@InputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @FPS => m_Wrapper.m_Default_FPS;
        public InputAction @Step => m_Wrapper.m_Default_Step;
        public InputAction @Zoom => m_Wrapper.m_Default_Zoom;
        public InputActionMap Get() { return m_Wrapper.m_Default; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(DefaultActions set) { return set.Get(); }
        public void SetCallbacks(IDefaultActions instance)
        {
            if (m_Wrapper.m_DefaultActionsCallbackInterface != null)
            {
                @FPS.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFPS;
                @FPS.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFPS;
                @FPS.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnFPS;
                @Step.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStep;
                @Step.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStep;
                @Step.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnStep;
                @Zoom.started -= m_Wrapper.m_DefaultActionsCallbackInterface.OnZoom;
                @Zoom.performed -= m_Wrapper.m_DefaultActionsCallbackInterface.OnZoom;
                @Zoom.canceled -= m_Wrapper.m_DefaultActionsCallbackInterface.OnZoom;
            }
            m_Wrapper.m_DefaultActionsCallbackInterface = instance;
            if (instance != null)
            {
                @FPS.started += instance.OnFPS;
                @FPS.performed += instance.OnFPS;
                @FPS.canceled += instance.OnFPS;
                @Step.started += instance.OnStep;
                @Step.performed += instance.OnStep;
                @Step.canceled += instance.OnStep;
                @Zoom.started += instance.OnZoom;
                @Zoom.performed += instance.OnZoom;
                @Zoom.canceled += instance.OnZoom;
            }
        }
    }
    public DefaultActions @Default => new DefaultActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard + Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface IDefaultActions
    {
        void OnFPS(InputAction.CallbackContext context);
        void OnStep(InputAction.CallbackContext context);
        void OnZoom(InputAction.CallbackContext context);
    }
}
