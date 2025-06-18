using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public enum GameState
{
    MenuState,
    PlayerSelectionState,
    SurpriseBoxState,
    PlaceItemState,
    MainGameState,
    ScoreState,
    FinalScoreState
}

public static class GameEvents
{
    //—— Current state tracking ——//
    private static GameState _currentState = GameState.MenuState;
    public static GameState CurrentState => _currentState;

    //—— Global state‐change event ——//
    public static event Action<GameState, GameState> OnStateChanged;

    //—— Per‐state “enter” events ——//
    public static event Action OnMenuStateEntered;
    
    public static event Action OnPlayerSelectionStateEntered;
    public static event Action OnPlayerSelectionStateExited;

    
    public static event Action OnSurpriseBoxStateEntered;
    public static event Action OnPlaceItemStateEntered;
    
    public static event Action OnMainGameStateEntered;
    public static event Action OnMainGameStateExited;

    public static event Action OnScoreStateEntered;

    public static event Action OnFinalScoreStateEntered;

    //—— Other events ——//
    public static event Action OnItemSelectionPanelOpened;
    public static event Action OnToggleGrid;
    public static event Action<PlayerInput> OnPlayerEliminated;

    //—— State‐change method ——//
    public static void ChangeState(GameState newState)
    {
        if (_currentState == newState)
            return;
        
        
        // 1) Fire exit for the old state
        switch (_currentState)
        {
            case GameState.PlayerSelectionState:
                OnPlayerSelectionStateExited?.Invoke();
                break;
            case GameState.MainGameState:
                OnMainGameStateExited?.Invoke();
                break;
            // case GameState.MenuState:     OnMenuStateExited?.Invoke();     break;
            // ...etc for other states if you want exit hooks
        }
        
        var oldState = _currentState;
        _currentState = newState;
        
        Debug.Log("State Change to: " + _currentState);
        
        // 2) Fire global event
        OnStateChanged?.Invoke(oldState, newState);

        // 3) Fire specific “enter” event
        switch (newState)
        {
            case GameState.MenuState:
                OnMenuStateEntered?.Invoke();
                break;
            case GameState.PlayerSelectionState:
                OnPlayerSelectionStateEntered?.Invoke();
                break;
            case GameState.SurpriseBoxState:
                OnSurpriseBoxStateEntered?.Invoke();
                break;
            case GameState.PlaceItemState:
                OnPlaceItemStateEntered?.Invoke();
                break;
            case GameState.MainGameState:
                OnMainGameStateEntered?.Invoke();
                break;
            case GameState.ScoreState:
                OnScoreStateEntered?.Invoke();
                break;
            case GameState.FinalScoreState:
                OnFinalScoreStateEntered?.Invoke();
                break;
        }
    }

    //—— Other existing invokers ——//
    public static void ItemSelectionPanelOpened() =>
        OnItemSelectionPanelOpened?.Invoke();

    public static void ToggleGrid() =>
        OnToggleGrid?.Invoke();
    
    public static void PlayerEliminated(PlayerInput p) 
        => OnPlayerEliminated?.Invoke(p);
}