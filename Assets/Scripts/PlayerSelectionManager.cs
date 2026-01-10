using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerSelectionManager : MonoBehaviour
{
    // For UI Manager and SurpriseBoxState
    public static event Action OnReturnToMainMenu;
    public static event Action OnStartGameSFX;
    public static event Action OnPlayerReadySFX;

    public static event Action OnNotAllPlayersReady;
    public static event Action OnNobodyJoinedYet;

    private readonly Dictionary<PlayerInput, PlayerSelectionData> _playerSelection =
        new Dictionary<PlayerInput, PlayerSelectionData>();

    // using the A and B UI button colors
    [SerializeField] private Color readyColor    = new Color();  // green-ish : 39EF07
    [SerializeField] private Color notReadyColor = new Color();  // red-ish : CF0000 using the A and B button colors

    [SerializeField] private GameObject PlayerSelection;
    [SerializeField] private GameObject MainMenu;
    
    
    void Awake()
    {
        PlayerManager.OnPlayerJoinedGlobal += HandlePlayerJoined;
        PlayerManager.OnPlayerLeftGlobal   += HandlePlayerLeft;
        TarodevController.PlayerController.OnPlayerReady += HandlePlayerReady;
        TarodevController.PlayerController.OnTryStartGame += TryStartGame;
        GameEvents.OnPlayerSelectionStateExited += HandlePlayerSelectionStateExit;
        TarodevController.PlayerController.OnReturnToMainMenu += HandleReturnToMainMenu;
        UIController.OnCancelPressed += HandleReturnToMainMenu;
        UIController.OnSubmitPressed += TryStartGame;
    }

    void OnDestroy()
    {
        PlayerManager.OnPlayerJoinedGlobal -= HandlePlayerJoined;
        PlayerManager.OnPlayerLeftGlobal   -= HandlePlayerLeft;
        TarodevController.PlayerController.OnPlayerReady -= HandlePlayerReady;
        TarodevController.PlayerController.OnTryStartGame -= TryStartGame;
        GameEvents.OnPlayerSelectionStateExited -= HandlePlayerSelectionStateExit;
        TarodevController.PlayerController.OnReturnToMainMenu -= HandleReturnToMainMenu;
        UIController.OnCancelPressed -= HandleReturnToMainMenu;
        UIController.OnSubmitPressed -= TryStartGame;

    }

    private void HandlePlayerJoined(PlayerInput playerInput, Transform characterTf)
    {
        var readyTf = characterTf.Find("ReadyText");
        var readyTMP = readyTf.GetComponent<TextMesh>();

        var data = new PlayerSelectionData
        {
            IsReady = false,
            CharacterTransform = characterTf,
            ReadyText = readyTMP
        };
        
        _playerSelection[playerInput] = data;
        
        data.ReadyText.text = "Not Ready";
        data.ReadyText.color = notReadyColor;
        
        Debug.Log(
            $"Player joined: input={playerInput.playerIndex}, " +
            $"isReady={data.IsReady}, " +
            $"characterTf={data.CharacterTransform.name}, " +
            $"readyText=\"{data.ReadyText.text}\" color={data.ReadyText.color}",
            characterTf   // optional context so clicking the log selects this object
        );
    }

    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        _playerSelection.Remove(playerInput);
    }
    
    
    private void HandlePlayerReady(PlayerInput playerInput)
    {
        if (!_playerSelection.TryGetValue(playerInput, out var data))
            return;

        data.IsReady = !data.IsReady;

        if (data.IsReady)
        {
            OnPlayerReadySFX?.Invoke();
        }
        if (data.ReadyText != null)
        {
            data.ReadyText.text = data.IsReady ? "Ready" : "Not Ready";
            data.ReadyText.color = data.IsReady ? readyColor : notReadyColor;

        }
        _playerSelection[playerInput] = data;

    }

    private void TryStartGame()
    {
        if (_playerSelection.Count == 0)
        {
            OnNobodyJoinedYet?.Invoke();
            return; 
        }

        bool everyoneReady = _playerSelection.Values.All(p => p.IsReady);

        if (!everyoneReady)
        {
            OnNotAllPlayersReady?.Invoke();
            return;
        }

        // otherwise the event gets called once from TarovDevController and once from UIController
        OnStartGameSFX?.Invoke();
        if (GameEvents.CurrentState == GameState.PlayerSelectionState) GameEvents.ChangeState(GameState.SurpriseBoxState);
    }

    private void HandlePlayerSelectionStateExit()
    {
        foreach (var data in _playerSelection.Values)
        {
            if (data.ReadyText != null)
                data.ReadyText.text = string.Empty;
        }

        _playerSelection.Clear();
    }

    private void HandleReturnToMainMenu()
    {
        if (GameEvents.CurrentState != GameState.PlayerSelectionState) return;
        
        OnReturnToMainMenu?.Invoke();
        PlayerSelection.SetActive(false);
        MainMenu.SetActive(true);
    }
    
}

public struct PlayerSelectionData
{
    public bool IsReady;
    public Transform CharacterTransform;
    public TextMesh ReadyText;   // or TextMeshProUGUI
}