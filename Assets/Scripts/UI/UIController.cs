using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static event Action OnCancelPressed;
    public static event Action OnSubmitPressed;
    
    public static event Action OnOptionMenuBackButtonPressed;

    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Button optionMenuBackButton;

    private InputSystemUIInputModule _uiModule;

    private void Awake()
    {
        GetEventSystem();

        // Subscribe to the cancel InputAction
        _uiModule.cancel.action.performed += OnCancel;
        _uiModule.submit.action.performed += OnSubmit;
        DontDestroyOnLoad(gameObject);
        //GameEvents.OnMenuStateEntered += GetEventSystem;

    }

    private void OnDestroy()
    {
        if (_uiModule != null)
        {
            _uiModule.cancel.action.performed -= OnCancel;
            _uiModule.submit.action.performed -= OnSubmit;
        }
        //GameEvents.OnMenuStateEntered -= GetEventSystem;

    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
            OnCancelPressed?.Invoke();
        if (GameEvents.CurrentState == GameState.MenuState)
        {
            optionMenuBackButton.onClick.Invoke();
            OnOptionMenuBackButtonPressed?.Invoke();
        }
    }
    
    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
            OnSubmitPressed?.Invoke();

    }
    
    private void GetEventSystem()
    {
        eventSystem = EventSystem.current;

        _uiModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (_uiModule == null)
        {
            Debug.LogError("No InputSystemUIInputModule on EventSystem", eventSystem);
        }
    }
}