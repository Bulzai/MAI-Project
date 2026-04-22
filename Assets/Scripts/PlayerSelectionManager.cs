using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PlayerSelectionManager : MonoBehaviour
{
    // For UI Manager and SurpriseBoxState
    public static event Action OnReturnToMainMenu;
    public static event Action OnStartGameSFX;
    public static event Action OnPlayerReadySFX;
    public static event Action OnSurpriseBoxStateTransitionStarted;
    public static event Action OnNotAllPlayersReady;
    public static event Action OnNobodyJoinedYet;

    [SerializeField] private Animator transitionAnimator;

    private bool _isTransitionRunning = false;

    private readonly Dictionary<PlayerInput, PlayerSelectionData> _playerSelection =
        new Dictionary<PlayerInput, PlayerSelectionData>();

    // using the A and B UI button colors
    [SerializeField] private Color readyColor = new Color();
    [SerializeField] private Color notReadyColor = new Color();

    [SerializeField] private GameObject PlayerSelection;
    [SerializeField] private GameObject MainMenu;

    [Header("UI Elements (Order: P1, P2, P3, P4)")]
    [Tooltip("Zieh hier die 'Press A' Objekte rein")]
    [SerializeField] private GameObject[] joinButtonsInstructions;
    [SerializeField] private GameObject[] PressTextInstructions;

    [Tooltip("Zieh hier die 'Press Y' Objekte rein")]
    [SerializeField] private GameObject[] readyInstructions;

    void Awake()
    {
        PlayerManager.OnPlayerJoinedGlobal += HandlePlayerJoined;
        PlayerManager.OnPlayerLeftGlobal += HandlePlayerLeft;
        TarodevController.PlayerController.OnPlayerReady += HandlePlayerReady;
        TarodevController.PlayerController.OnTryStartGame += TryStartGame;
        GameEvents.OnPlayerSelectionStateExited += HandlePlayerSelectionStateExit;
        TarodevController.PlayerController.OnReturnToMainMenu += HandleReturnToMainMenu;
        UIController.OnCancelPressed += HandleReturnToMainMenu;

        ResetUI();
    }

    private void ResetUI()
    {
        if (joinButtonsInstructions != null)
            foreach (var obj in joinButtonsInstructions)
                if (obj) obj.SetActive(true);

        if (PressTextInstructions != null)
            foreach (var obj in PressTextInstructions)
                if (obj) obj.SetActive(true);

        if (readyInstructions != null)
            foreach (var obj in readyInstructions)
                if (obj) obj.SetActive(false);
    }

    void OnDestroy()
    {
        PlayerManager.OnPlayerJoinedGlobal -= HandlePlayerJoined;
        PlayerManager.OnPlayerLeftGlobal -= HandlePlayerLeft;
        TarodevController.PlayerController.OnPlayerReady -= HandlePlayerReady;
        TarodevController.PlayerController.OnTryStartGame -= TryStartGame;
        GameEvents.OnPlayerSelectionStateExited -= HandlePlayerSelectionStateExit;
        TarodevController.PlayerController.OnReturnToMainMenu -= HandleReturnToMainMenu;
        UIController.OnCancelPressed -= HandleReturnToMainMenu;
    }

    private void HandlePlayerJoined(PlayerInput playerInput, Transform characterTf)
    {
        var readyTf = characterTf.Find("ReadyText");
        TextMesh readyTMP = null;

        if (readyTf != null)
            readyTMP = readyTf.GetComponent<TextMesh>();

        var data = new PlayerSelectionData
        {
            IsReady = false,
            CharacterTransform = characterTf,
            ReadyText = readyTMP
        };

        _playerSelection[playerInput] = data;

        if (data.ReadyText != null)
        {
            data.ReadyText.text = " ";
            data.ReadyText.color = notReadyColor;
        }

        int pIndex = playerInput.playerIndex;

        if (joinButtonsInstructions != null && pIndex < joinButtonsInstructions.Length)
            joinButtonsInstructions[pIndex].SetActive(false);

        if (readyInstructions != null && pIndex < readyInstructions.Length)
            readyInstructions[pIndex].SetActive(true);

        Debug.Log($"Player joined: input={playerInput.playerIndex}");
    }

    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        int pIndex = playerInput.playerIndex;

        if (joinButtonsInstructions != null && pIndex < joinButtonsInstructions.Length)
            joinButtonsInstructions[pIndex].SetActive(true);

        if (PressTextInstructions != null && pIndex < PressTextInstructions.Length)
            PressTextInstructions[pIndex].SetActive(true);

        if (readyInstructions != null && pIndex < readyInstructions.Length)
            readyInstructions[pIndex].SetActive(false);

        _playerSelection.Remove(playerInput);
    }

    private void HandlePlayerReady(PlayerInput playerInput)
    {
        if (!_playerSelection.TryGetValue(playerInput, out var data))
            return;

        data.IsReady = !data.IsReady;

        if (data.IsReady)
            OnPlayerReadySFX?.Invoke();

        if (data.ReadyText != null)
        {
            data.ReadyText.text = data.IsReady ? "Ready" : " ";
            data.ReadyText.color = data.IsReady ? readyColor : notReadyColor;
        }

        int pIndex = playerInput.playerIndex;

        if (readyInstructions != null && pIndex < readyInstructions.Length)
            readyInstructions[pIndex].SetActive(!data.IsReady);

        if (PressTextInstructions != null && pIndex < PressTextInstructions.Length)
            PressTextInstructions[pIndex].SetActive(!data.IsReady);

        _playerSelection[playerInput] = data;

        TryStartGame();
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

        if (_isTransitionRunning)
            return;

        OnStartGameSFX?.Invoke();
        StartCoroutine(TransitionToSurpriseBox());
    }

    private IEnumerator TransitionToSurpriseBox()
    {
        _isTransitionRunning = true;

        Image transitionImage = transitionAnimator.GetComponent<Image>();
        if (transitionImage != null)
            transitionImage.enabled = true;

        if (transitionAnimator != null)
            transitionAnimator.SetTrigger("Play");

        OnSurpriseBoxStateTransitionStarted?.Invoke();

        yield return new WaitForSeconds(1.1f);

        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
        {
            GameEvents.ChangeState(GameState.SurpriseBoxState);
        }

        yield return new WaitForSeconds(0.45f);

        _isTransitionRunning = false;

        if (transitionImage != null)
            transitionImage.enabled = false;
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
        if (GameEvents.CurrentState != GameState.PlayerSelectionState)
            return;

        OnReturnToMainMenu?.Invoke();

        if (PlayerSelection != null)
            PlayerSelection.SetActive(false);

        if (MainMenu != null)
            MainMenu.SetActive(true);
    }
}

public struct PlayerSelectionData
{
    public bool IsReady;
    public Transform CharacterTransform;
    public TextMesh ReadyText;
}