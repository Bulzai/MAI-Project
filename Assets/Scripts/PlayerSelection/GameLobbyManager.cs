using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLobbyManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject joinPrompt;
    [SerializeField] private GameObject readyPrompt;
    [SerializeField] private GameObject startPrompt;
    [SerializeField] private GameObject playerListPanel;
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Player Settings")]
    [SerializeField] private int maxPlayers = 4;

    private Dictionary<int, bool> playerReadyStates = new Dictionary<int, bool>();
    private Dictionary<int, GameObject> playerListItems = new Dictionary<int, GameObject>();

    private LobbyState currentState = LobbyState.WaitingForPlayers;

    private enum LobbyState
    {
        WaitingForPlayers,
        PlayersJoined,
        AllReady,
        StartingGame
    }

    void Start()
    {
        // Start with no players
        UpdateUI();
    }

    void OnEnable()
    {
        // Subscribe to your GameEvents for state changes
        GameEvents.OnPlayerSelectionStateEntered += OnEnterLobby;
        GameEvents.OnPlayerSelectionStateExited += OnExitLobby;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerSelectionStateEntered -= OnEnterLobby;
        GameEvents.OnPlayerSelectionStateExited -= OnExitLobby;
    }

    private void OnEnterLobby()
    {
        // Reset lobby state when entering
        ResetLobby();
        UpdateUI();
    }

    private void OnExitLobby()
    {
        // Clean up when leaving lobby
        ClearPlayerList();
    }

    public void OnPlayerJoined(int playerId)
    {
        if (playerReadyStates.ContainsKey(playerId))
            return;

        playerReadyStates[playerId] = false; // Start as NOT READY

        Debug.Log($"Player {playerId} joined. Setting initial state: NOT READY");

        // Update the existing slot to show "NOT READY!"
        UpdatePlayerListItem(playerId);

        UpdateLobbyState();
        UpdateUI();
    }

    public void OnPlayerLeft(int playerId)
    {
        if (playerReadyStates.ContainsKey(playerId))
        {
            playerReadyStates.Remove(playerId);

            // Reset the existing slot to "NOT READY!"
            UpdatePlayerListItem(playerId);

            UpdateLobbyState();
            UpdateUI();
        }
    }

    public void SetPlayerReady(int playerId, bool isReady)
    {
        Debug.Log($"GameLobbyManager: SetPlayerReady({playerId}, {isReady}) called");

        if (playerReadyStates.ContainsKey(playerId))
        {
            playerReadyStates[playerId] = isReady;
            Debug.Log($"Player {playerId} ready state set to: {playerReadyStates[playerId]}");

            UpdatePlayerListItem(playerId);
            UpdateLobbyState();
            UpdateUI();
        }
    }

    public void TogglePlayerReady(int playerId)
    {
        Debug.Log($"GameLobbyManager: TogglePlayerReady({playerId}) called");

        if (playerReadyStates.ContainsKey(playerId))
        {
            playerReadyStates[playerId] = !playerReadyStates[playerId];
            Debug.Log($"Player {playerId} ready state toggled to: {playerReadyStates[playerId]}");

            UpdatePlayerListItem(playerId);
            UpdateLobbyState();
            UpdateUI();
        }
    }

    // Called when player presses A to start (from input system)
    public void TryStartGame()
    {
        if (currentState == LobbyState.AllReady)
        {
            StartGame();
        }
        else
        {
            ShowFeedback("Not everyone is ready!");
        }
    }

    // Called when player presses B to return (from input system)
    public void ReturnToMenu()
    {
        GameEvents.ChangeState(GameState.MenuState);
    }

    private void UpdateLobbyState()
    {
        if (playerReadyStates.Count == 0)
        {
            currentState = LobbyState.WaitingForPlayers;
            return;
        }

        bool allReady = true;
        foreach (var state in playerReadyStates.Values)
        {
            if (!state)
            {
                allReady = false;
                break;
            }
        }

        currentState = allReady ? LobbyState.AllReady : LobbyState.PlayersJoined;
    }

    private void StartGame()
    {
        if (playerReadyStates.Count == 0)
        {
            ShowFeedback("Cannot start: No players joined!");
            return;
        }

        currentState = LobbyState.StartingGame;

        // Use YOUR existing event system to start the game
        GameEvents.ChangeState(GameState.SurpriseBoxState);

        // Hide lobby UI
        if (playerListPanel != null)
            playerListPanel.SetActive(false);
    }

    private void UpdatePlayerListItem(int playerId)
    {
        // CHANGE TO: Find existing slot instead
        string slotName = GetPlayerSlotName(playerId);
        GameObject playerSlot = GameObject.Find(slotName);

        if (playerSlot != null)
        {
            PlayerListItemUI itemUI = playerSlot.GetComponent<PlayerListItemUI>();
            if (itemUI != null)
            {
                itemUI.SetReady(playerReadyStates[playerId]);
                Debug.Log($"Updated existing player slot: {slotName}");
            }
            else
            {
                Debug.LogError($"No PlayerListItemUI on {slotName}");
            }
        }
        else
        {
            Debug.LogError($"Could not find player slot: {slotName}");
        }
    }

    private string GetPlayerSlotName(int playerId)
    {
        switch (playerId)
        {
            case 0: return "player_one";
            case 1: return "player_two";
            case 2: return "player_three";
            case 3: return "player_four";
            default: return "";
        }
    }

    private void ClearPlayerList()
    {
        foreach (var item in playerListItems.Values)
        {
            if (item != null)
                Destroy(item);
        }
        playerListItems.Clear();
        playerReadyStates.Clear();
    }

    private void UpdateUI()
    {
        // Keep buttons always visible (as per your request)
        // Just update the player list visibility
        if (playerListPanel != null)
            playerListPanel.SetActive(playerReadyStates.Count > 0);
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);

            // Hide after 2 seconds
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 2f);
        }
    }

    private void HideFeedback()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
    }

    private void ResetLobby()
    {
        currentState = LobbyState.WaitingForPlayers;
        ClearPlayerList();
    }
}