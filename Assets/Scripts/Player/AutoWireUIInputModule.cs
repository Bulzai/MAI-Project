using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
public class AutoWireUIInputModule : MonoBehaviour
{
    private PlayerInput _playerInput;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        TryWireEventSystem();
    }

    void OnEnable()
    {
        // In case EventSystem appears later (scene load order)
        TryWireEventSystem();
    }

    private void TryWireEventSystem()
    {
        // Use the currently active EventSystem
        var es = EventSystem.current;
        if (es == null)
        {
            Debug.LogWarning("AutoWireUIInputModule: No active EventSystem found.", this);
            return;
        }

        var uiModule = es.GetComponent<InputSystemUIInputModule>();
        if (uiModule == null)
        {
            Debug.LogWarning("AutoWireUIInputModule: EventSystem has no InputSystemUIInputModule.", es);
            return;
        }

        _playerInput.uiInputModule = uiModule;
        Debug.Log($"AutoWireUIInputModule: Wired UI module from EventSystem '{es.name}' to PlayerInput on '{name}'.", this);
    }
}