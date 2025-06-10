using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;


public class PlayerManager : MonoBehaviour
{
    public Transform[] SpawnPoints;

    public List<PlayerInput> players = new List<PlayerInput>();
    private List<PlayerInput> _eliminationOrder = new List<PlayerInput>();

    private int PlayerCount;

    // ----------- WORK IN PROGRESS
    private Dictionary<string, GameObject> PlayerItems;
    // -----------

    
    private PlayerInputManager playerInputManager;
    private void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerSelectionStateEntered += EnablePlayerJoining;
        GameEvents.OnPlayerSelectionStateExited += DeactivePlayerPrefab;
        GameEvents.OnPlayerSelectionStateExited += DisablePlayerJoining;
        
        GameEvents.OnMainGameStateEntered += ActivatePlayerPrefab;
        GameEvents.OnMainGameStateExited += DeactivePlayerPrefab;

        
        GameEvents.OnPlayerEliminated += HandlePlayerElimination;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerSelectionStateEntered -= EnablePlayerJoining;
        GameEvents.OnPlayerSelectionStateExited -= DeactivePlayerPrefab;
        GameEvents.OnMainGameStateEntered -= ActivatePlayerPrefab;
        GameEvents.OnMainGameStateExited -= DeactivePlayerPrefab;
        GameEvents.OnPlayerSelectionStateEntered -= DisablePlayerJoining;
        GameEvents.OnPlayerEliminated -= HandlePlayerElimination;

    }
    public void ResetEliminations()
    {
        _eliminationOrder.Clear();
    }
    private void HandlePlayerElimination(PlayerInput p)
    {
        
        Debug.Log($"Player {p.playerIndex} was eliminated");
        // Record elimination
        _eliminationOrder.Add(p);

        int aliveCount = players.Count - _eliminationOrder.Count;
        if (aliveCount <= 1)
        {
            Debug.Log("last alive");

            // the last survivor:
            var winner = players.Except(_eliminationOrder).FirstOrDefault();
            if (winner != null)
                _eliminationOrder.Add(winner);

            // now go to the score screen
            GameEvents.ChangeState(GameState.ScoreState);
        }
    }
    
    public IReadOnlyList<PlayerInput> GetRoundRanking()
    {
        // _eliminationOrder is [first out, second out, …, winner]
        // we want [winner, 2nd place, 3rd, …]
        return _eliminationOrder
            .AsEnumerable()
            .Reverse()
            .ToList();
    }
    public void EnablePlayerJoining()
    {
        playerInputManager.EnableJoining();
    }
    public void DisablePlayerJoining()
    {
        playerInputManager.DisableJoining();
    }
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (players.Contains(playerInput))
            return;
        
        playerInput.transform.position = SpawnPoints[PlayerCount].transform.position;
        PlayerCount++;
        players.Add(playerInput);

        int playerIndex = playerInput.playerIndex;
        playerInput.gameObject.name = $"Player {playerIndex + 1}";

        Debug.Log($"Player joined haha: {playerInput.gameObject.name}");

        // ----------- WORK IN PROGRESS
        // Subscribe to the click event on this player's actions
        playerInput.actions["Choose Item"].performed += context => OnClick(playerInput, context);
        // -----------

        //playerInput.DeactivateInput();
    }

    public void DeactivePlayerPrefab()
    {
        foreach (PlayerInput p in players)
        {
            p.gameObject.SetActive(false);
        }
    }

    public void ActivatePlayerPrefab()
    {
        foreach (PlayerInput p in players.ToList())
        {
            Debug.Log($"Activating player {p.gameObject.name}");
            p.gameObject.SetActive(true);
            p.ActivateInput();

            PlayerHealthSystem playerHealthSystem = p.GetComponent<PlayerHealthSystem>();
            playerHealthSystem.currentHealth = playerHealthSystem.maxHealth;
            playerHealthSystem.isBurning = false;
            playerHealthSystem.SetOnFire();
        }
    }

    // ----------- WORK IN PROGRESS
    private void OnClick(PlayerInput playerInput, InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var clickedItem = hit.collider.gameObject;
            // Now you know playerInput (the player) and the clickedItem
            Debug.Log($"Player {playerInput.playerIndex} clicked {clickedItem.name}");
        }
    }
    // -----------
}
