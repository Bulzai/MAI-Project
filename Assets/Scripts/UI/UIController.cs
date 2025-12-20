using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UIController : MonoBehaviour
{
    public static event Action OnCancelPressed;

    
    [SerializeField] private EventSystem eventSystem;

    private InputSystemUIInputModule _uiModule;

    private void Awake()
    {
        if (eventSystem == null)
            eventSystem = EventSystem.current;

        _uiModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (_uiModule == null)
        {
            Debug.LogError("No InputSystemUIInputModule on EventSystem", eventSystem);
            return;
        }

        // Subscribe to the cancel InputAction
        _uiModule.cancel.action.performed += OnCancelPerformed;
        DontDestroyOnLoad(gameObject);

    }

    private void OnDestroy()
    {
        if (_uiModule != null)
            _uiModule.cancel.action.performed -= OnCancelPerformed;
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
            OnCancelPressed?.Invoke();
        
        Debug.Log("UI Cancel pressed");
    }
}