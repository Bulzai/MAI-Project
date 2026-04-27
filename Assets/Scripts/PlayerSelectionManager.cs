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
    public static event Action OnPlayerSelectionCountDownStarted;
    public static event Action OnPlayerSelectionCountDownStopped;
    [SerializeField] private TMP_Text countdownText;
    private Coroutine countdownRoutine;
    
    [SerializeField] private Animator transitionAnimator;

    bool _isTransitionRunning = false;
    private readonly Dictionary<PlayerInput, PlayerSelectionData> _playerSelection =
        new Dictionary<PlayerInput, PlayerSelectionData>();

    // using the A and B UI button colors
    [SerializeField] private Color readyColor    = new Color();  // green-ish : 39EF07
    [SerializeField] private Color notReadyColor = new Color();  // red-ish : CF0000 using the A and B button colors

    [SerializeField] private GameObject PlayerSelection;
    [SerializeField] private GameObject MainMenu;

    [SerializeField] private GameObject[] pressTexts; // the "PRESS" word

    [Header("UI Elements (Order: P1, P2, P3, P4)")]
    [SerializeField] private GameObject[] joinButtonsInstructions; // X button
    [SerializeField] private GameObject[] joinTextInstructions;    // "TO JOIN"

    [SerializeField] private GameObject[] readyButtonsInstructions; // Y button
    [SerializeField] private GameObject[] readyTextInstructions;    // "TO GET READY"

    void Awake()
    {
        PlayerManager.OnPlayerJoinedGlobal += HandlePlayerJoined;
        PlayerManager.OnPlayerLeftGlobal   += HandlePlayerLeft;
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

        if (joinTextInstructions != null)
            foreach (var obj in joinTextInstructions)
                if (obj) obj.SetActive(true);

        if (readyButtonsInstructions != null)
            foreach (var obj in readyButtonsInstructions)
                if (obj) obj.SetActive(false);

        if (readyTextInstructions != null)
            foreach (var obj in readyTextInstructions)
                if (obj) obj.SetActive(false);

        if (pressTexts != null)
            foreach (var obj in pressTexts)
                if (obj) obj.SetActive(true);
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

        data.ReadyText.text = " ";
        data.ReadyText.color = notReadyColor;

        //  UI Logik für Join ---
        int pIndex = playerInput.playerIndex;

        // hide X + "TO JOIN"
        if (joinButtonsInstructions != null && pIndex < joinButtonsInstructions.Length)
            joinButtonsInstructions[pIndex].SetActive(false);

        if (joinTextInstructions != null && pIndex < joinTextInstructions.Length)
            joinTextInstructions[pIndex].SetActive(false);

        // show Y + "TO GET READY"
        if (readyButtonsInstructions != null && pIndex < readyButtonsInstructions.Length)
            readyButtonsInstructions[pIndex].SetActive(true);

        if (readyTextInstructions != null && pIndex < readyTextInstructions.Length)
            readyTextInstructions[pIndex].SetActive(true);

        if (pressTexts != null && pIndex < pressTexts.Length)
            pressTexts[pIndex].SetActive(true);
        // ------------------------------

        Debug.Log($"Player joined: input={playerInput.playerIndex}");
        StopCountdownIfRunning();
    }

    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        // --- NEU: UI zurücksetzen wenn Spieler geht ---
        int pIndex = playerInput.playerIndex;

        // show X + "TO JOIN"
        if (joinButtonsInstructions != null && pIndex < joinButtonsInstructions.Length)
            joinButtonsInstructions[pIndex].SetActive(true);

        if (joinTextInstructions != null && pIndex < joinTextInstructions.Length)
            joinTextInstructions[pIndex].SetActive(true);

        // hide Y + "TO GET READY"
        if (readyButtonsInstructions != null && pIndex < readyButtonsInstructions.Length)
            readyButtonsInstructions[pIndex].SetActive(false);

        if (readyTextInstructions != null && pIndex < readyTextInstructions.Length)
            readyTextInstructions[pIndex].SetActive(false);

        if (pressTexts != null && pIndex < pressTexts.Length)
            pressTexts[pIndex].SetActive(true);
        // ---------------------------------------------

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

            data.ReadyText.text = data.IsReady ? "Ready" : " ";
            data.ReadyText.color = data.IsReady ? readyColor : notReadyColor;

        }

        // --- UI Instruction ("Press Y") togglen ---
        int pIndex = playerInput.playerIndex;

        // if ready: hide Y + "TO GET READY"
        // if not ready: show Y + "TO GET READY"
        if (readyButtonsInstructions != null && pIndex < readyButtonsInstructions.Length)
            readyButtonsInstructions[pIndex].SetActive(!data.IsReady);

        if (readyTextInstructions != null && pIndex < readyTextInstructions.Length)
            readyTextInstructions[pIndex].SetActive(!data.IsReady);

        if (pressTexts != null && pIndex < pressTexts.Length)
            pressTexts[pIndex].SetActive(!data.IsReady);
        // -----------------------------------------------

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
            StopCountdownIfRunning();
            OnNotAllPlayersReady?.Invoke();
            return;
        }
        
        // 2. Die Transition-Sequenz starten
        StartEnterCountdown();
        OnPlayerSelectionCountDownStarted?.Invoke();
    }

    private IEnumerator TransitionToSurpriseBox()
    {
        _isTransitionRunning = true;
        // Vorbereitung: Image enablen & Animation starten
        // (Ich nehme an, transitionAnimator ist in dieser Klasse bekannt)
        Image transitionImage = transitionAnimator.GetComponent<Image>();
        transitionImage.enabled = true;
        transitionAnimator.SetTrigger("Play");
        OnSurpriseBoxStateTransitionStarted?.Invoke();
        // 3. Warten, bis die Transition den Bildschirm verdeckt (deine 1.1s)
        yield return new WaitForSeconds(1.1f);

        // 4. State-Wechsel genau JETZT ausf�hren

        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
        {
            GameEvents.ChangeState(GameState.SurpriseBoxState);
        }

        // 5. Kurz warten, damit der neue State geladen/initialisiert ist
        yield return new WaitForSeconds(0.45f);
        _isTransitionRunning = false;
        // 6. Transition wieder unsichtbar machen
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
        if (GameEvents.CurrentState != GameState.PlayerSelectionState) return;
        StopCountdownIfRunning();
        OnReturnToMainMenu?.Invoke();
        PlayerSelection.SetActive(false);
        MainMenu.SetActive(true);
    }
    
    
    //countdown stuff
    private void StopCountdownIfRunning()
    {
        OnPlayerSelectionCountDownStopped?.Invoke();
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }
    
    private void StartEnterCountdown()
    {
        StopCountdownIfRunning();
        countdownRoutine = StartCoroutine(PlayCountdown(OnCountdownFinished));
    }

    private void OnCountdownFinished()
    {
        if (_isTransitionRunning) return;
        StartCoroutine(TransitionToSurpriseBox());
        OnStartGameSFX?.Invoke();
    }

    public IEnumerator PlayCountdown(Action onFinished, int seconds = 3, float timing = 1f)
    {
        float countdown = seconds;
        countdownText.gameObject.SetActive(true);

        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(timing);
            countdown--;
        }

        countdownText.gameObject.SetActive(false);
        countdownRoutine = null;
        onFinished?.Invoke();  // Only called on successful finish
    }

    
}

public struct PlayerSelectionData
{
    public bool IsReady;
    public Transform CharacterTransform;
    public TextMesh ReadyText;   // or TextMeshProUGUI
}