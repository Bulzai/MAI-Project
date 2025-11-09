using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;
using TarodevController;


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public PlayerAnimator playerAnimator;

    public Transform[] spawnPositionsForGame;

    public List<PlayerInput> players = new List<PlayerInput>();
    private List<PlayerInput> _eliminationOrder = new List<PlayerInput>();

    public int playerCount = 0;


    [Header("Avatars")]
    public Sprite[] playerAvatars = new Sprite[4];  // set per slot in Inspector
    public CharacterAnimationSet[] animationSets = new CharacterAnimationSet[4];

    [Header("Player Colors")]
    public Color[] playerColors = new Color[4];
    [Header("Spawn Positions")]
    public Transform[] spawnPositionsForMenu;
    public Transform[] spawnPositionsForItemPlacement;
    public Transform[] spawnPositionsForItemSelection;


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


        GameEvents.OnMainGameStateEntered += ResetEliminations;
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

    public void SetCharacter(CharacterAnimationSet chosenSet)
    {
        playerAnimator.animationSet = chosenSet;
    }

    public void ResetEliminations()
    {
        _eliminationOrder.Clear();
    }
    private void HandlePlayerElimination(PlayerInput p)
    {
        if (p == null) return;
        if (_eliminationOrder.Contains(p)) return;

        // Record elimination
        _eliminationOrder.Add(p);

        int aliveCount = players.Count - _eliminationOrder.Count;
        if (aliveCount <= 1)
        {

            // the last survivor:
            var winner = players.Except(_eliminationOrder).FirstOrDefault();
            if (winner != null)
                _eliminationOrder.Add(winner);

            // now go to the score screen
            GameEvents.ChangeState(GameState.ScoreState);
        }
    }
    private IEnumerator HandlePlayerEliminationCoroutine(PlayerInput p)
    {
        Debug.Log($"Player {p.playerIndex} was eliminated");

        // 1. Play death particles
        var root = p.gameObject;
        var particlesTf = root.transform.Find("PlayerNoPI/Visual/Particles/Death Animation");

        if (particlesTf != null)
        {
            var ps = particlesTf.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                // 2. Wait until the particle system is done
                yield return new WaitWhile(() => ps.IsAlive());
            }
        }

        // 3. Record elimination
        _eliminationOrder.Add(p);

        int aliveCount = players.Count - _eliminationOrder.Count;
        if (aliveCount <= 1)
        {
            Debug.Log("last alive");

            // Add last survivor
            var winner = players.Except(_eliminationOrder).FirstOrDefault();
            if (winner != null)
                _eliminationOrder.Add(winner);

            // Change state *after* animation
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

        playerCount++;
        players.Add(playerInput);

        int idx = playerInput.playerIndex;

        GameObject root = playerInput.gameObject;
        root.name = "Player_" + idx;

        // Find children
        var cursorTf = root.transform.Find("CursorNoPI");
        var characterTf = root.transform.Find("PlayerNoPI");

        if (cursorTf == null || characterTf == null)
        {
            Debug.LogError("Root prefab missing CursorNoPI or PlayerNoPI");
            return;
        }

        // Position at selection/menu spawns
        if (idx < spawnPositionsForMenu.Length)
            characterTf.transform.position = spawnPositionsForMenu[idx].position;
        else
            characterTf.transform.position = Vector3.one;

        if (idx < spawnPositionsForItemPlacement.Length)
            cursorTf.transform.position = spawnPositionsForItemPlacement[idx].position;
        else
            cursorTf.transform.position = Vector3.one;

        cursorTf.GetComponent<CursorController>()
            .SetBoundsSuprisoeBoxState(GameObject.Find("CursorBoundsSurpriseBoxState").GetComponent<BoxCollider2D>());
        
        cursorTf.GetComponent<CursorController>()
            .SetBoundsPlaceItemState(GameObject.Find("CursorBoundsPlaceItemState").GetComponent<BoxCollider2D>());

        // Hide cursor until placement phase
        cursorTf.gameObject.SetActive(false);

        // Cache root
        playerRoots[idx] = root;

        // ----- Assign AVATAR + COLOR -----
        var characterSpriteRenderer = characterTf.Find("Visual/Sprite")?.GetComponent<SpriteRenderer>();
        var cursorSpriteRenderer = cursorTf.GetComponent<SpriteRenderer>();

        if (characterSpriteRenderer != null)
        {
            // Avatar per player slot
            if (playerAvatars != null && idx < playerAvatars.Length && playerAvatars[idx] != null)
                characterSpriteRenderer.sprite = playerAvatars[idx];

            // Optional: tint per player slot
            /*if (playerColors != null && idx < playerColors.Length)
                characterSpriteRenderer.color = playerColors[idx];
            else
                characterSpriteRenderer.color = Color.white;*/
        }
        else
        {
            Debug.LogWarning($"SpriteRenderer not found for Player {idx} at Visual/Sprite");
        }

        if (cursorSpriteRenderer != null)
        {
            if (playerColors != null && idx < playerColors.Length)
                cursorSpriteRenderer.color = playerColors[idx];
        }
        else
        {
            Debug.LogWarning($"CursorSpriteRenderer not found for Player {idx}");
        }

        // ------------ASSIGN CHARACTER ANIMATION SET---------------------
        var characterAnimator = characterTf.Find("Visual")?.GetComponent<PlayerAnimator>();
        if (characterAnimator != null && idx < animationSets.Length)
        {
            characterAnimator.animationSet = animationSets[idx];
        }

        // Setup input
        var pi = root.GetComponent<PlayerInput>();
        pi.SwitchCurrentActionMap("Player");
        pi.ActivateInput();
    }



    public void OnPlayerLeft(PlayerInput pi)
    {
        int idx = pi.playerIndex;
        playerCount = Mathf.Max(0, playerCount - 1);

        players.Remove(pi);

        if (playerRoots.TryGetValue(idx, out var root))
        {
            Destroy(root);
            playerRoots.Remove(idx);
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
            health.spriteRenderer.color = health.originalColor;
            health.currentHealth = health.maxHealth;
            health.isBurning = false;
            health.SetOnFire();

            // **NEW:** Reposition character at spawn point
            if (idx < spawnPositionsForGame.Length)
            {
                characterGO.transform.position = spawnPositionsForGame[idx].position;
            }
            else
            {
                Debug.LogWarning($"No placement spawn defined for player {idx}, using default.");
                characterGO.transform.position = Vector3.zero;
            }
        }
    }


    public void ResetCursorPositionItemPlacement(int idx)
    {
        if (playerRoots.TryGetValue(idx, out var root))
        {
            var cursor = root.transform.Find("CursorNoPI");
            if (idx < spawnPositionsForItemPlacement.Length)
                cursor.position = spawnPositionsForItemPlacement[idx].position;
        }
    }

    public void ResetCursorPositionItemSelection(int idx)
    {
        if (playerRoots.TryGetValue(idx, out var root))
        {
            var cursor = root.transform.Find("CursorNoPI");
            if (idx < spawnPositionsForItemSelection.Length)
                cursor.position = spawnPositionsForItemSelection[idx].position;
        }
    }

}
