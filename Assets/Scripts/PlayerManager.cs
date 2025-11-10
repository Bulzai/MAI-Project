using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
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

    private static bool IsDestroyed(Object o) => o == null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        if (playerAnimator != null) playerAnimator.animationSet = chosenSet;
    }

    public void ResetEliminations()
    {
        _eliminationOrder.Clear();
    }

    private void HandlePlayerElimination(PlayerInput p)
    {
        if (IsDestroyed(p)) return;
        if (_eliminationOrder.Contains(p)) return;

        _eliminationOrder.Add(p);

        PruneDestroyedPlayers();

        int aliveCount = players.Count - _eliminationOrder.Count;
        if (aliveCount <= 1)
        {
            // last survivor:
            var winner = players.Where(pi => !_eliminationOrder.Contains(pi) && !IsDestroyed(pi)).FirstOrDefault();
            if (winner != null) _eliminationOrder.Add(winner);

            GameEvents.ChangeState(GameState.ScoreState);
        }
    }

    private IEnumerator HandlePlayerEliminationCoroutine(PlayerInput p)
    {
        if (!IsDestroyed(p))
            Debug.Log($"Player {p.playerIndex} was eliminated");

        var root = !IsDestroyed(p) ? p.gameObject : null;
        var particlesTf = root != null ? root.transform.Find("PlayerNoPI/Visual/Particles/Death Animation") : null;

        if (particlesTf != null)
        {
            var ps = particlesTf.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                yield return new WaitWhile(() => ps.IsAlive());
            }
        }

        if (!IsDestroyed(p)) _eliminationOrder.Add(p);

        PruneDestroyedPlayers();

        int aliveCount = players.Count - _eliminationOrder.Count;
        if (aliveCount <= 1)
        {
            Debug.Log("last alive");
            var winner = players.Where(pi => !_eliminationOrder.Contains(pi) && !IsDestroyed(pi)).FirstOrDefault();
            if (winner != null) _eliminationOrder.Add(winner);

            GameEvents.ChangeState(GameState.ScoreState);
        }
    }

    public IReadOnlyList<PlayerInput> GetRoundRanking()
    {
        // _eliminationOrder is [first out, ..., winner], return reversed but minus destroyed refs
        return _eliminationOrder
            .Where(pi => !IsDestroyed(pi))
            .Reverse()
            .ToList();
    }

    public void EnablePlayerJoining()
    {
        if (playerInputManager != null) playerInputManager.EnableJoining();
    }

    public void DisablePlayerJoining()
    {
        if (playerInputManager != null) playerInputManager.DisableJoining();
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (IsDestroyed(playerInput)) return;

        PruneDestroyedPlayers();

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

        var cursorCtrl = cursorTf.GetComponent<CursorController>();
        if (cursorCtrl != null)
        {
            var box1 = GameObject.Find("CursorBoundsSurpriseBoxState")?.GetComponent<BoxCollider2D>();
            var box2 = GameObject.Find("CursorBoundsPlaceItemState")?.GetComponent<BoxCollider2D>();
            if (box1 != null) cursorCtrl.SetBoundsSuprisoeBoxState(box1);
            if (box2 != null) cursorCtrl.SetBoundsPlaceItemState(box2);
        }

        // Hide cursor until placement phase
        cursorTf.gameObject.SetActive(false);

        // Cache root
        playerRoots[idx] = root;

        // ----- Assign AVATAR + COLOR -----
        var characterSpriteRenderer = characterTf.Find("Visual/Sprite")?.GetComponent<SpriteRenderer>();
        var cursorSpriteRenderer = cursorTf.GetComponent<SpriteRenderer>();

        if (characterSpriteRenderer != null)
        {
            if (playerAvatars != null && idx < playerAvatars.Length && playerAvatars[idx] != null)
                characterSpriteRenderer.sprite = playerAvatars[idx];
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
        if (pi != null)
        {
            pi.SwitchCurrentActionMap("Player");
            pi.ActivateInput();
        }
    }

    public void OnPlayerLeft(PlayerInput pi)
    {
        if (IsDestroyed(pi)) return;
        int idx = pi.playerIndex;

        // Reuse shared cleanup
        CleanupPlayerBookkeeping(idx, pi);

        if (playerRoots.TryGetValue(idx, out var root) && root != null)
        {
            Destroy(root);
            playerRoots.Remove(idx);
        }

        pickedPrefabByPlayer.Remove(idx);
        playersThatPlaced.Remove(idx);
    }

    public void DeactivateCharacterPrefab()
    {
        foreach (var root in playerRoots.Values.ToArray())
        {
            if (IsDestroyed(root)) continue;
            var characterTf = root.transform.Find("PlayerNoPI");
            if (characterTf != null)
            {
                var character = characterTf.gameObject;
                character.SetActive(false);
            }

            var pi = root.GetComponent<PlayerInput>();
            if (pi != null) pi.DeactivateInput();
        }
    }

    public void ActivateCharacterPrefab()
    {
        foreach (var kvp in playerRoots.ToArray())
        {
            int idx = kvp.Key;
            GameObject root = kvp.Value;
            if (IsDestroyed(root)) continue;

            var characterTf = root.transform.Find("PlayerNoPI");
            if (characterTf == null) continue;

            var characterGO = characterTf.gameObject;
            characterGO.SetActive(true);

            var pi = root.GetComponent<PlayerInput>();
            if (pi != null)
            {
                pi.ActivateInput();
                pi.SwitchCurrentActionMap("Player");
            }

            var health = characterGO.GetComponent<PlayerHealthSystem>();
            if (health != null)
            {
                if (health.spriteRenderer != null)
                    health.spriteRenderer.color = health.originalColor;

                health.currentHealth = health.maxHealth;
                health.isBurning = false;
                // If SetOnFire() actually sets burning, consider renaming;
                // keeping your call to preserve behavior.
                health.SetOnFire();
            }

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
        if (playerRoots.TryGetValue(idx, out var root) && root != null)
        {
            var cursor = root.transform.Find("CursorNoPI");
            if (cursor != null && idx < spawnPositionsForItemPlacement.Length)
                cursor.position = spawnPositionsForItemPlacement[idx].position;
        }
    }

    public void ResetCursorPositionItemSelection(int idx)
    {
        if (playerRoots.TryGetValue(idx, out var root) && root != null)
        {
            var cursor = root.transform.Find("CursorNoPI");
            if (cursor != null && idx < spawnPositionsForItemSelection.Length)
                cursor.position = spawnPositionsForItemSelection[idx].position;
        }
    }

    // -------------------- RESET PIPELINE --------------------

    public void HardResetGame()
    {
        // Stop any player-related coroutines (like elimination animations)
        StopAllCoroutines();

        // Block joins during reset
        DisablePlayerJoining();

        // Quiet the characters first
        try { DeactivateCharacterPrefab(); } catch { }

        // Kick off coroutine to ensure Destroy() flushes this frame
        StartCoroutine(HardResetRoutine());
    }

    private IEnumerator HardResetRoutine()
    {
        // Snapshot: we’ll mutate 'players' while removing
        var snapshot = players.ToArray();
        foreach (var pi in snapshot)
        {
            ForceRemovePlayer_NoManager(pi);
        }

        // Also nuke any lingering PlayerInput objects not tracked in our lists
        NukeAllPlayerObjects();

        // Clear all runtime state
        _eliminationOrder.Clear();
        players.Clear();
        playerRoots.Clear();
        pickedPrefabByPlayer.Clear();
        playersThatPlaced.Clear();
        playerCount = 0;

        // Let Destroy() process
        yield return null;

        // Final sanitization
        PruneDestroyedPlayers();

        // Optionally allow re-joining immediately:
        // EnablePlayerJoining();
    }

    /// <summary>
    /// Finds all PlayerInput objects everywhere (active, inactive, DDOL) and destroys their roots.
    /// </summary>
    private void NukeAllPlayerObjects()
    {
        var allPlayerInputs = Resources.FindObjectsOfTypeAll<PlayerInput>();
        foreach (var pi in allPlayerInputs)
        {
            if (IsDestroyed(pi)) continue;

            // Skip assets/prefabs (not scene instances)
            var go = pi.gameObject;
            var scene = go.scene;
            if (!scene.IsValid() || !scene.isLoaded) continue;

            try { pi.DeactivateInput(); } catch { }
            try { if (pi.user.valid) pi.user.UnpairDevices(); } catch { }

            var root = go.transform.root?.gameObject;
            if (!IsDestroyed(root))
            {
                Destroy(root);
            }
        }
    }

    /// <summary>
    /// Fully removes a player WITHOUT relying on PlayerInputManager.RemovePlayer:
    /// - Deactivates input
    /// - Unpairs devices (no ghost input after destroy)
    /// - Destroys the player root
    /// - Mirrors your bookkeeping
    /// </summary>
    private void ForceRemovePlayer_NoManager(PlayerInput pi)
    {
        if (IsDestroyed(pi)) return;

        int idx = pi.playerIndex;

        // 1) Stop any input from this player
        try { pi.DeactivateInput(); } catch { }
        try
        {
            if (pi.user.valid)
            {
                pi.user.UnpairDevices();
            }
        }
        catch { }

        // 2) Bookkeeping (same behavior as OnPlayerLeft)
        CleanupPlayerBookkeeping(idx, pi);

        // 3) Destroy the root object
        if (playerRoots.TryGetValue(idx, out var root) && root != null)
        {
            Destroy(root);
        }
        else
        {
            Destroy(pi.gameObject);
        }

        // 4) Remove all references
        playerRoots.Remove(idx);
        pickedPrefabByPlayer.Remove(idx);
        playersThatPlaced.Remove(idx);
    }

    /// <summary>
    /// Shared bookkeeping so we don't duplicate logic.
    /// </summary>
    private void CleanupPlayerBookkeeping(int idx, PlayerInput pi)
    {
        playerCount = Mathf.Max(0, playerCount - 1);
        players.Remove(pi);
    }

    /// <summary>
    /// Remove destroyed refs from lists/dicts to avoid MissingReferenceException.
    /// </summary>
    public void PruneDestroyedPlayers()
    {
        players.RemoveAll(p => IsDestroyed(p));

        // Clean up dead roots
        var keys = playerRoots.Keys.ToArray();
        foreach (var k in keys)
        {
            if (!playerRoots[k]) playerRoots.Remove(k);
        }

        // Also ensure elimination order has no dead refs
        _eliminationOrder = _eliminationOrder.Where(pi => !IsDestroyed(pi)).ToList();
    }
}
