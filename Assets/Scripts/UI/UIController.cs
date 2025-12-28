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
    public static event Action OnSubmitPressed;
    public static event Action OnEnableInputUIController;
    
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
        _uiModule.cancel.action.performed += OnCancel;
        _uiModule.submit.action.performed += OnSubmit;
        DontDestroyOnLoad(gameObject);
        GameEvents.OnFinalScoreStateEntered += OnEnableInputUIController;

    }

    private void OnDestroy()
    {
        if (_uiModule != null)
        {
            _uiModule.cancel.action.performed -= OnCancel;
            _uiModule.submit.action.performed -= OnSubmit;
        }
        GameEvents.OnFinalScoreStateEntered -= OnEnableInputUIController;

    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
            OnCancelPressed?.Invoke();
        Debug.Log("OnCancelPressed UIController");
    }
    
    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
            OnSubmitPressed?.Invoke();

    }
}