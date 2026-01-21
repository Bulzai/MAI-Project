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


    [Header("UI Elements (Order: P1, P2, P3, P4)")]
    [Tooltip("Zieh hier die 'Press A' Objekte rein")]
    [SerializeField] private GameObject[] joinButtonsInstructions;
    [SerializeField] private GameObject[] PressTextInstructions;

    [Tooltip("Zieh hier die 'Press Y' Objekte rein")]
    [SerializeField] private GameObject[] readyInstructions;

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
        // Sicherstellen, dass am Anfang alles richtig steht
        if (joinButtonsInstructions != null)
            foreach (var obj in joinButtonsInstructions) if (obj) obj.SetActive(true);

        if (PressTextInstructions != null)
            foreach (var obj in PressTextInstructions) if (obj) obj.SetActive(true);

        if (readyInstructions != null)
            foreach (var obj in readyInstructions) if (obj) obj.SetActive(false);
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
        int pIndex = playerInput.playerIndex; // Das ist 0, 1, 2 oder 3

        // "Press A" ausschalten
        if (joinButtonsInstructions != null && pIndex < joinButtonsInstructions.Length)
        {
            joinButtonsInstructions[pIndex].SetActive(false);
        }

        // "Press Y" einschalten
        if (readyInstructions != null && pIndex < readyInstructions.Length)
        {
            readyInstructions[pIndex].SetActive(true);
        }
        // ------------------------------

        Debug.Log($"Player joined: input={playerInput.playerIndex}");
        StopCountdownIfRunning();
    }

    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        // --- NEU: UI zurücksetzen wenn Spieler geht ---
        int pIndex = playerInput.playerIndex;

        if (joinButtonsInstructions != null && pIndex < joinButtonsInstructions.Length)
            joinButtonsInstructions[pIndex].SetActive(true); // A wieder anzeigen

        if (PressTextInstructions != null && pIndex < PressTextInstructions.Length)
            PressTextInstructions[pIndex].SetActive(true); // A wieder anzeigen

        if (readyInstructions != null && pIndex < readyInstructions.Length)
            readyInstructions[pIndex].SetActive(false); // Y ausblenden
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
        if (readyInstructions != null && pIndex < readyInstructions.Length)
        {
            // Wenn er Ready ist -> Text weg. 
            // Wenn er NICHT Ready ist -> Text da (damit er weiß, dass er Y drücken kann).
            readyInstructions[pIndex].SetActive(!data.IsReady);
        }
        if (PressTextInstructions != null && pIndex < PressTextInstructions.Length)
        {
            // Wenn er Ready ist -> Text weg. 
            // Wenn er NICHT Ready ist -> Text da (damit er weiß, dass er Y drücken kann).
            PressTextInstructions[pIndex].SetActive(!data.IsReady);
        }
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
        
        OnReturnToMainMenu?.Invoke();
        PlayerSelection.SetActive(false);
        MainMenu.SetActive(true);
    }
    
    
    //countdown stuff
    private void StopCountdownIfRunning()
    {
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