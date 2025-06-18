using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;


public class PlayerManagerFinal : MonoBehaviour
{
    public static PlayerManagerFinal Instance { get; private set; }

    public Transform[] SpawnPoints;

    public List<PlayerInput> players = new List<PlayerInput>();
    private List<PlayerInput> _eliminationOrder = new List<PlayerInput>();

    public int PlayerCount;

    [Header("Spawn Positions")]
    public Transform[] spawnPositionsForSelection;
    public Transform[] spawnPositionsForPlacement;


    public Dictionary<int, GameObject> playerRoots = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> pickedPrefabByPlayer = new Dictionary<int, GameObject>();
    public HashSet<int> playersThatPlaced = new HashSet<int>();


    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("PlayerManager.Awake -> setting Instance");

    }

    private void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerSelectionStateEntered += EnablePlayerJoining;
        GameEvents.OnPlayerSelectionStateExited += DeactivateCharacterPrefab;
        GameEvents.OnPlayerSelectionStateExited += DisablePlayerJoining;

        GameEvents.OnMainGameStateEntered += ActivateCharacterPrefab;
        GameEvents.OnMainGameStateExited += DeactivateCharacterPrefab;


        GameEvents.OnPlayerEliminated += HandlePlayerElimination;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerSelectionStateEntered -= EnablePlayerJoining;
        GameEvents.OnPlayerSelectionStateExited -= DeactivateCharacterPrefab;
        GameEvents.OnMainGameStateEntered -= ActivateCharacterPrefab;
        GameEvents.OnMainGameStateExited -= DeactivateCharacterPrefab;
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

        PlayerCount++;
        players.Add(playerInput);

        //int playerIndex = playerInput.playerIndex;
        //playerInput.gameObject.name = $"Player {playerIndex + 1}";


        //playerInput.DeactivateInput();

        int idx = playerInput.playerIndex;
        Debug.Log("OnPlayerJoined idx=" + idx + " totalJoined=" + PlayerCount);

        // pi.gameObject is already the instantiated root prefab
        GameObject root = playerInput.gameObject;
        root.name = "PlayerRoot_" + idx;

        // find children
        var cursorTf = root.transform.Find("CursorNoPIFinal");
        var characterTf = root.transform.Find("PlayerNoPI");
        if (cursorTf == null || characterTf == null)
        {
            Debug.LogError("Root prefab missing CursorNoPI or PlayerNoPI");
            return;
        }
        // position at selection spawn
        if (idx < SpawnPoints.Length)
            characterTf.transform.position = SpawnPoints[idx].transform.position;
        else
            characterTf.transform.position = Vector3.one;

        if (idx < spawnPositionsForSelection.Length)
            cursorTf.transform.position = spawnPositionsForSelection[idx].transform.position;
        else
            cursorTf.transform.position = Vector3.one;
        
        // hide character until placement
        cursorTf.gameObject.SetActive(false);

        playerRoots[idx] = root;
        Debug.Log("Cached PlayerRoot for idx=" + idx);
        root.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
        root.GetComponent<PlayerInput>().ActivateInput();
    }

    public void OnPlayerLeft(PlayerInput pi)
    {
        int idx = pi.playerIndex;
        PlayerCount = Mathf.Max(0, PlayerCount - 1);
        Debug.Log("OnPlayerLeft idx=" + idx + " nowJoined=" + PlayerCount);

        if (playerRoots.TryGetValue(idx, out var root))
        {
            Destroy(root);
            playerRoots.Remove(idx);
            Debug.Log("Destroyed PlayerRoot for idx=" + idx);
        }

        pickedPrefabByPlayer.Remove(idx);
        playersThatPlaced.Remove(idx);
    }

    public void DeactivateCharacterPrefab()
    {
        foreach (var root in playerRoots.Values)
        {
            var character = root.transform.Find("PlayerNoPI").gameObject;
            character.SetActive(false);
            root.GetComponent<PlayerInput>().DeactivateInput();

        }
    }

    public void ActivateCharacterPrefab()
    {
        Debug.Log("ActivatePlayerPrefab called");

        foreach (var kvp in playerRoots)
        {
            int idx = kvp.Key;
            GameObject root = kvp.Value;

            // Activate character & input
            var characterGO = root.transform.Find("PlayerNoPI").gameObject;
            characterGO.SetActive(true);
            var pi = root.GetComponent<PlayerInput>();
            pi.ActivateInput();
            pi.SwitchCurrentActionMap("Player");

            // Reset health & state
            var health = characterGO.GetComponent<PlayerHealthSystem>();
            health.currentHealth = health.maxHealth;
            health.isBurning = false;
            health.SetOnFire();

            // **NEW:** Reposition character at spawn point
            if (idx < spawnPositionsForPlacement.Length)
            {
                characterGO.transform.position = spawnPositionsForPlacement[idx].position;
            }
            else
            {
                Debug.LogWarning($"No placement spawn defined for player {idx}, using default.");
                characterGO.transform.position = Vector3.zero;
            }
        }
    }




}
