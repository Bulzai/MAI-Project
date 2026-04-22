using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Unity.VisualScripting;
using TarodevController;
using Object = UnityEngine.Object;

public class PlayerManager : MonoBehaviour
{
    public static event Action<PlayerInput, Transform> OnPlayerJoinedGlobal;
    public static event Action<PlayerInput> OnPlayerLeftGlobal;
    public static event Action OnPlayerJoinedSFX;

    public static PlayerManager Instance { get; private set; }

    public PlayerAnimator playerAnimator;

    public Transform[] spawnPositionsForGame;
    public List<PlayerInput> players = new List<PlayerInput>();
    private List<PlayerInput> _eliminationOrder = new List<PlayerInput>();

    public int playerCount = 0;

    [Header("Avatars")]
    public Sprite[] playerAvatars = new Sprite[4];
    public Sprite[] playerCursors = new Sprite[4];
    public CharacterAnimationLibrary[] characterLibraries = new CharacterAnimationLibrary[4];

    [Header("PlayerJoin Names")]
    public Transform[] namePositionsInJoinMenu;

    [Header("Spawn Positions")]
    public Transform[] spawnPositionsForMenu;
    public Transform[] spawnPositionsForItemPlacement;
    public Transform[] spawnPositionsForItemSelection;

    public Dictionary<int, GameObject> playerRoots = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> pickedPrefabByPlayer = new Dictionary<int, GameObject>();
    private Dictionary<InputDevice, int> deviceToPlayerMap = new Dictionary<InputDevice, int>();
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
        GameEvents.OnPlayerSelectionStateExited -= DisablePlayerJoining;

        GameEvents.OnMainGameStateEntered -= ResetEliminations;
        GameEvents.OnMainGameStateEntered -= ActivateCharacterPrefab;
        GameEvents.OnMainGameStateExited -= DeactivateCharacterPrefab;

        GameEvents.OnPlayerEliminated -= HandlePlayerElimination;
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
            var winner = players.Where(pi => !_eliminationOrder.Contains(pi) && !IsDestroyed(pi)).FirstOrDefault();
            if (winner != null) _eliminationOrder.Add(winner);
            StartCoroutine(TransitionToScoreState());
        }
    }

    private IEnumerator TransitionToScoreState()
    {
        yield return new WaitForSeconds(2f);
        GameEvents.ChangeState(GameState.ScoreState);
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

        switch (idx)
        {
            case 0: root.name = "Cutesy"; break;
            case 1: root.name = "Jokesy"; break;
            case 2: root.name = "xX_GamerL0rd_Xx"; break;
            case 3: root.name = "Currywurst"; break;
        }

        var cursorTf = root.transform.Find("CursorNoPI");
        var characterTf = root.transform.Find("PlayerNoPI");

        if (cursorTf == null || characterTf == null)
        {
            Debug.LogError("Root prefab missing CursorNoPI or PlayerNoPI");
            return;
        }

        if (idx < spawnPositionsForMenu.Length)
        {
            characterTf.transform.position = spawnPositionsForMenu[idx].position;
            namePositionsInJoinMenu[idx].gameObject.SetActive(true);
        }
        else
        {
            characterTf.transform.position = Vector3.one;
        }

        OnPlayerJoinedGlobal?.Invoke(playerInput, characterTf);
        OnPlayerJoinedSFX?.Invoke();

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

        cursorTf.gameObject.SetActive(false);
        playerRoots[idx] = root;

        var characterSpriteRenderer = characterTf.Find("Visual/Sprite")?.GetComponent<SpriteRenderer>();
        var cursorSpriteRenderer = cursorTf.GetComponentInChildren<SpriteRenderer>();

        if (characterSpriteRenderer != null)
        {
            if (playerAvatars != null && idx < playerAvatars.Length && playerAvatars[idx] != null)
                characterSpriteRenderer.sprite = playerAvatars[idx];
        }

        if (cursorSpriteRenderer != null)
        {
            if (playerCursors != null && idx < playerCursors.Length && playerCursors[idx] != null)
                cursorSpriteRenderer.sprite = playerCursors[idx];
        }
        else
        {
            Debug.LogWarning($"CursorSpriteRenderer not found for Player {idx}");
        }

        var characterAnimator = characterTf.Find("Visual")?.GetComponent<PlayerAnimator>();
        if (characterAnimator != null)
        {
            characterAnimator.SetLibrary(characterLibraries[idx]);
        }

        var pi = root.GetComponent<PlayerInput>();
        if (pi != null)
        {
            pi.SwitchCurrentActionMap("Player");
            pi.ActivateInput();
        }

        if (playerInput.devices.Count > 0)
        {
            deviceToPlayerMap[playerInput.devices[0]] = idx;
            Debug.Log($"Player {idx} is using device: {playerInput.devices[0].name}");
        }
    }

    public void OnPlayerLeft(PlayerInput pi)
    {
        if (IsDestroyed(pi)) return;
        int idx = pi.playerIndex;

        CleanupPlayerBookkeeping(idx, pi);

        if (playerRoots.TryGetValue(idx, out var root) && root != null)
        {
            Destroy(root);
            playerRoots.Remove(idx);
        }

        pickedPrefabByPlayer.Remove(idx);
        playersThatPlaced.Remove(idx);

        OnPlayerLeftGlobal?.Invoke(pi);
        Destroy(pi);
        Debug.Log("player left");
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

            var health = characterGO.GetComponent<PlayerHealthSystem>();

            if (health != null)
            {
                health.ResetDeathFlags();

                if (health.spriteRenderer != null)
                    health.spriteRenderer.color = health.originalColor;

                health.currentHealth = health.maxHealth;
                health.isBurning = false;
                health.SetOnFire();
            }

            var pi = root.GetComponent<PlayerInput>();
            if (pi != null)
            {
                pi.ActivateInput();
                pi.SwitchCurrentActionMap("Player");
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

    public void HardResetGame()
    {
        StopAllCoroutines();
        DisablePlayerJoining();

        try { DeactivateCharacterPrefab(); } catch { }

        StartCoroutine(HardResetRoutine());
    }

    private IEnumerator HardResetRoutine()
    {
        var snapshot = players.ToArray();
        foreach (var pi in snapshot)
        {
            ForceRemovePlayer_NoManager(pi);
        }

        NukeAllPlayerObjects();

        _eliminationOrder.Clear();
        players.Clear();
        playerRoots.Clear();
        pickedPrefabByPlayer.Clear();
        playersThatPlaced.Clear();
        playerCount = 0;

        yield return null;

        PruneDestroyedPlayers();
    }

    private void NukeAllPlayerObjects()
    {
        var allPlayerInputs = Resources.FindObjectsOfTypeAll<PlayerInput>();
        foreach (var pi in allPlayerInputs)
        {
            if (IsDestroyed(pi)) continue;

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

    private void ForceRemovePlayer_NoManager(PlayerInput pi)
    {
        if (IsDestroyed(pi)) return;

        int idx = pi.playerIndex;

        try { pi.DeactivateInput(); } catch { }
        try
        {
            if (pi.user.valid)
            {
                pi.user.UnpairDevices();
            }
        }
        catch { }

        CleanupPlayerBookkeeping(idx, pi);

        if (playerRoots.TryGetValue(idx, out var root) && root != null)
        {
            Destroy(root);
        }
        else
        {
            Destroy(pi.gameObject);
        }

        playerRoots.Remove(idx);
        pickedPrefabByPlayer.Remove(idx);
        playersThatPlaced.Remove(idx);
    }

    private void CleanupPlayerBookkeeping(int idx, PlayerInput pi)
    {
        playerCount = Mathf.Max(0, playerCount - 1);
        players.Remove(pi);
    }

    public void PruneDestroyedPlayers()
    {
        players.RemoveAll(p => IsDestroyed(p));

        var keys = playerRoots.Keys.ToArray();
        foreach (var k in keys)
        {
            if (!playerRoots[k]) playerRoots.Remove(k);
        }

        _eliminationOrder = _eliminationOrder.Where(pi => !IsDestroyed(pi)).ToList();
    }

    public void HardResetFinalScore()
    {
        StartCoroutine(HardResetRoutine());
    }
}