using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Root Prefab")]
    public GameObject playerRootPrefab;       // must contain CursorNoPI & PlayerNoPI children

    [Header("Spawn Positions")]
    public Vector3[] spawnPositionsForSelection;
    public Vector3[] spawnPositionsForPlacement;

    [Header("Background Swap")]
    public SpriteRenderer backgroundRenderer;
    public Sprite selectionBackgroundSprite;
    public Sprite placementBackgroundSprite;

    [SerializeField]
    private PlayerInputManager inputManager;

    private int playersThatJoined = 0;
    private Dictionary<int, GameObject> playerRoots = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> pickedPrefabByPlayer = new Dictionary<int, GameObject>();
    private HashSet<int> playersThatPlaced = new HashSet<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("GameManager.Awake -> setting Instance");

    }

    // Called by PlayerInputManager when a new player joins
    public void OnPlayerJoined(PlayerInput pi)
    {
        int idx = pi.playerIndex;
        playersThatJoined++;
        Debug.Log("OnPlayerJoined idx=" + idx + " totalJoined=" + playersThatJoined);

        // pi.gameObject is already the instantiated root prefab
        GameObject root = pi.gameObject;
        root.name = "PlayerRoot_" + idx;

        // position at selection spawn
        if (idx < spawnPositionsForSelection.Length)
            root.transform.position = spawnPositionsForSelection[idx];
        else
            root.transform.position = Vector3.zero;

        // find children
        var cursorTf = root.transform.Find("CursorNoPI");
        var characterTf = root.transform.Find("PlayerNoPI");
        if (cursorTf == null || characterTf == null)
        {
            Debug.LogError("Root prefab missing CursorNoPI or PlayerNoPI");
            return;
        }

        // hide character until placement
        characterTf.gameObject.SetActive(false);

        playerRoots[idx] = root;
        Debug.Log("Cached PlayerRoot for idx=" + idx);
    }

    // Called by PlayerInputManager when a player leaves
    public void OnPlayerLeft(PlayerInput pi)
    {
        int idx = pi.playerIndex;
        playersThatJoined = Mathf.Max(0, playersThatJoined - 1);
        Debug.Log("OnPlayerLeft idx=" + idx + " nowJoined=" + playersThatJoined);

        if (playerRoots.TryGetValue(idx, out var root))
        {
            Destroy(root);
            playerRoots.Remove(idx);
            Debug.Log("Destroyed PlayerRoot for idx=" + idx);
        }

        pickedPrefabByPlayer.Remove(idx);
        playersThatPlaced.Remove(idx);
    }

    // Called by CursorController when a player picks an item
    public void NotifyPlayerPicked(int idx, GameObject prefab)
    {
        Debug.Log("NotifyPlayerPicked idx=" + idx + " prefab=" + prefab.name);
        if (!pickedPrefabByPlayer.ContainsKey(idx))
            pickedPrefabByPlayer[idx] = prefab;

        if (pickedPrefabByPlayer.Count == playersThatJoined)
        {
            Debug.Log("All picked → BeginPlacementPhaseAll");
            BeginPlacementPhaseAll();
        }
    }

    private void BeginPlacementPhaseAll()
    {
        Debug.Log("BeginPlacementPhaseAll start");

        if (pickedPrefabByPlayer.Count == 0)
        {
            Debug.Log("No picks yet, aborting");
            return;
        }

        if (backgroundRenderer != null && placementBackgroundSprite != null)
        {
            backgroundRenderer.sprite = placementBackgroundSprite;
            Debug.Log("Background set to placement");
        }

        playersThatPlaced.Clear();
        //GridPlacementSystem.Instance.ShowGrid();
        Debug.Log("Grid shown");

        foreach (var kv in pickedPrefabByPlayer)
        {
            int idx = kv.Key;
            GameObject prefab = kv.Value;

            if (!playerRoots.TryGetValue(idx, out var root))
            {
                Debug.LogError("Missing root for idx=" + idx);
                continue;
            }

            // enable cursor, disable character
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            var character = root.transform.Find("PlayerNoPI").gameObject;
            cursor.SetActive(true);
            character.SetActive(false);

            // position at placement spawn
            Vector3 pos = (idx < spawnPositionsForPlacement.Length)
                ? spawnPositionsForPlacement[idx]
                : Vector3.zero;
            cursor.transform.position = pos;
            Debug.Log("Cursor idx=" + idx + " moved to " + pos);

            // switch input map
            var pi = root.GetComponent<PlayerInput>();
            pi.SwitchCurrentActionMap("Cursor");
            Debug.Log("Switched to Cursor map for idx=" + idx);

            // begin placement on cursor controller
            var cc = cursor.GetComponent<CursorController>();
            cc.BeginPlacementPhase(prefab, pos);
            Debug.Log("BeginPlacementPhase on idx=" + idx);
        }

        Debug.Log("BeginPlacementPhaseAll end");
    }

    // Called by CursorController when a player places their item
    public void NotifyPlayerPlaced(int idx)
    {
        Debug.Log("NotifyPlayerPlaced idx=" + idx);

        if (!playersThatPlaced.Contains(idx))
            playersThatPlaced.Add(idx);

        // deactivate cursor
        if (playerRoots.TryGetValue(idx, out var root))
        {
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            cursor.SetActive(false);
            Debug.Log("Deactivated cursor idx=" + idx);
        }

        Debug.Log("placedCount=" + playersThatPlaced.Count +
                  " joinedCount=" + playersThatJoined);

        if (playersThatPlaced.Count == playersThatJoined)
        {
            Debug.Log("All placed → AllPlayersFinishedPlacing");
            AllPlayersFinishedPlacing();
        }
    }

    private void AllPlayersFinishedPlacing()
    {
        Debug.Log("AllPlayersFinishedPlacing start");

        GridPlacementSystem.Instance.HideGrid();
        pickedPrefabByPlayer.Clear();
        playersThatPlaced.Clear();

        if (backgroundRenderer != null && selectionBackgroundSprite != null)
        {
            backgroundRenderer.sprite = selectionBackgroundSprite;
            Debug.Log("Background reset to selection");
        }

        SpawnAllCharacters();
        Debug.Log("AllPlayersFinishedPlacing end");
    }

    // Reactivate every cursor
    public void ShowAllCursors()
    {
        Debug.Log("ShowAllCursors called");
        foreach (var root in playerRoots.Values)
        {
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            cursor.SetActive(true);
            root.GetComponent<PlayerInput>().SwitchCurrentActionMap("Cursor");
        }
    }

    // Reactivate every character
    public void SpawnAllCharacters()
    {
        Debug.Log("SpawnAllCharacters called");
        foreach (var root in playerRoots.Values)
        {
            var character = root.transform.Find("PlayerNoPI").gameObject;
            character.SetActive(true);
            root.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
        }
    }
}

/*// GameManager.cs
// Make sure you create an empty GameObject in your scene called "GameManager" and attach this script to it.
// Also assign your PlayerCursor prefab in the inspector.

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs & Settings")]
    public GameObject playerCursorPrefab; // assign: Prefabs/Player/PlayerCursor.prefab
    public Vector3[] spawnPositionsForSelection;  // e.g. { (-5,0,0), (5,0,0), ... } for initial cursors
    public Vector3[] spawnPositionsForPlacement;  // e.g. another set of positions for placement cursors
    public SpriteRenderer backgroundRenderer;      // assign the background so we can change sprite/colour
    [Header("Background Swap")]
    public Sprite selectionBackgroundSprite;
    public Sprite placementBackgroundSprite;
    int playersThatJoined = 0;

    // Internals:
    private Dictionary<int, GameObject> playerCursors = new Dictionary<int, GameObject>();
    // maps playerIndex -> that player's Cursor GameObject
    private Dictionary<int, GameObject> pickedPrefabByPlayer = new Dictionary<int, GameObject>();
    // maps playerIndex -> the prefab they chose
    private HashSet<int> playersThatPlaced = new HashSet<int>();


    [SerializeField] private PlayerInputManager inputManager;  // assign in Inspector


    private int maxPlayers = 4;

    [Header("Per player prefabs")]
    public GameObject[] cursorPrefabs;      // e.g. [redCursor, blueCursor, greenCursor…]
    public GameObject[] characterPrefabs;   // e.g. [redChar, blueChar, greenChar…]
    public Transform[] selectionSpawns;     // cursor spawn positions
    public Transform[] characterSpawns;     // character spawn positions
    public GameObject playerRootPrefab;           // assign in Inspector

    private Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

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
        // If players are already joined at Start (or you can wait until they press JOIN),
        // you could loop over PlayerInput.all and spawn cursors. But with PlayerInputManager,
        // we spawn each in OnPlayerJoined below.
        maxPlayers = 4;
        playersThatPlaced.Clear();
        pickedPrefabByPlayer.Clear();
    }
    /*
    // This gets called by PlayerInputManager when a new Player joins
    public void OnPlayerJoined(PlayerInput newPlayer)
    {
        // The PlayerInputManager has already spawned newPlayer.gameObject
        GameObject cursorGO = newPlayer.gameObject;
        int idx = newPlayer.playerIndex;
        cursorGO.name = $"Cursor_Player{idx}";

        // Position it
        if (idx < spawnPositionsForSelection.Length)
            cursorGO.transform.position = spawnPositionsForSelection[idx];
        else
            cursorGO.transform.position = Vector3.zero;

        // Cache it for later
        playerCursors[idx] = cursorGO;

        // Wire up your controller script
        var cc = cursorGO.GetComponent<CursorController>();
        cc.playerInput = newPlayer;
        playersThatJoined++;
    }

// Called by PlayerInputManager when a new player joins
public void OnPlayerJoined(PlayerInput pi)
    {
        int idx = pi.playerIndex;
        playersThatJoined++;

        // The manager has already instantiated pi.gameObject as the root prefab
        GameObject root = pi.gameObject;
        root.transform.position = spawnPositionsForSelection[idx];
        root.name = $"PlayerRoot_{idx}";

        // Find child CursorGO and CharacterGO
        GameObject cursor = root.transform.Find("CursorGO").gameObject;
        GameObject character = root.transform.Find("CharacterGO").gameObject;

        // Hide the character initially
        character.SetActive(false);

        // Store in dictionary
        players[idx] = new PlayerData
        {
            input = pi,
            cursorGO = cursor,
            characterGO = character,
            controllingCursor = true
        };
    }

    // Called by PlayerInputManager when a player leaves
    public void OnPlayerLeft(PlayerInput pi)
    {
        int idx = pi.playerIndex;
        playersThatJoined = Mathf.Max(0, playersThatJoined - 1);

        if (players.TryGetValue(idx, out var data))
        {
            Destroy(data.cursorGO);
            Destroy(data.characterGO);
            players.Remove(idx);
        }

        playersThatPlaced.Remove(idx);
        pickedPrefabByPlayer.Remove(idx);

        Debug.Log("Left: now " + playersThatJoined);
    }



    // Called by CursorController when a player successfully picks an item
    public void NotifyPlayerPicked(int playerIndex, GameObject prefabChosen)
    {

        if (!pickedPrefabByPlayer.ContainsKey(playerIndex))
        {
            pickedPrefabByPlayer[playerIndex] = prefabChosen;
        }

        // If ALL joined players have now picked, start placement phase:
        if (pickedPrefabByPlayer.Count == playersThatJoined )
        {
            BeginPlacementPhaseAll();
        }
        Debug.Log("player: " + playerIndex + "placed");
        Debug.Log("playercount: " + PlayerInput.all.Count);
        Debug.Log("playerprefabcount: " + pickedPrefabByPlayer.Count);
    }
    /*
    private void BeginPlacementPhaseAll()
    {
        // 1) Change background
        if (backgroundRenderer != null && placementBackgroundSprite != null)
        {
            backgroundRenderer.sprite = placementBackgroundSprite;
        }

        // 2) For each player, either reuse their existing cursor or spawn a fresh one:
        playersThatPlaced.Clear();

        for (int i = 0; i < PlayerInput.all.Count; i++)
        {
            int idx = PlayerInput.all[i].playerIndex;
            Vector3 spawnPos = (i < spawnPositionsForPlacement.Length)
                                ? spawnPositionsForPlacement[i]
                                : Vector3.zero;

            // If the original selection-cursor is still around but hidden, destroy it so we can make a clean placement-cursor:
            if (playerCursors.ContainsKey(idx) && playerCursors[idx] != null)
            {
                Destroy(playerCursors[idx]);
            }

            // 3) Spawn a brand-new cursor for placement:
            GameObject cursorGO = Instantiate(playerCursorPrefab, spawnPos, Quaternion.identity);
            cursorGO.name = "PlacementCursor_Player" + idx;
            CursorController cc = cursorGO.GetComponent<CursorController>();
            cc.playerInput = PlayerInput.all[i];

            // 4) Attach the prefab they chose under the cursor
            GameObject prefabToGive = pickedPrefabByPlayer[idx];
            cc.BeginPlacementPhase(prefabToGive, spawnPos);

            playerCursors[idx] = cursorGO;
        }
    }
    
    private void BeginPlacementPhaseAll()
    {
        Debug.Log(">>> BeginPlacementPhaseAll()");
        Debug.Log("pickedPrefabByPlayer.Count = " + pickedPrefabByPlayer.Count);

        // 1) Early bail if nothing to do
        if (pickedPrefabByPlayer.Count == 0)
        {
            Debug.LogError("pickedPrefabByPlayer is EMPTY! No one picked yet.");
            return;
        }

        // 2) Swap the background
        if (backgroundRenderer != null && placementBackgroundSprite != null)
            backgroundRenderer.sprite = placementBackgroundSprite;

        // 3) Clear placed-tracker
        playersThatPlaced.Clear();


        GridPlacementSystem.Instance.ShowGrid();

        Debug.Log("calling for each");

        // 4) Loop over each picked entry
        foreach (KeyValuePair<int, GameObject> kvp in pickedPrefabByPlayer)
        {
            int idx = kvp.Key;
            Debug.Log("» Processing playerIndex = " + idx);

            // 4a) Find PlayerInput
            PlayerInput matchingInput = null;
            for (int i = 0; i < PlayerInput.all.Count; i++)
            {
                if (PlayerInput.all[i].playerIndex == idx)
                {
                    matchingInput = PlayerInput.all[i];
                    break;
                }
            }
            if (matchingInput == null)
            {
                Debug.LogError("No PlayerInput found for index " + idx);
                continue;
            }
            Debug.Log("Found PlayerInput for " + idx);

            // 4b) Get existing cursor GameObject
            if (!playerCursors.ContainsKey(idx))
            {
                Debug.LogError("playerCursors has NO key for index " + idx);
                continue;
            }
            GameObject cursorGO = playerCursors[idx];
            if (cursorGO == null)
            {
                Debug.LogError("playerCursors[" + idx + "] is NULL");
                continue;
            }
            Debug.Log("cursorGO = " + cursorGO.name);

            // 4c) Get its CursorController
            CursorController cc = cursorGO.GetComponent<CursorController>();
            if (cc == null)
            {
                Debug.LogError("CursorController missing on " + cursorGO.name);
                continue;
            }
            Debug.Log("CursorController OK");

            // 4d) Pick spawn position
            int loopIndex = 0;
            for (int i = 0; i < PlayerInput.all.Count; i++)
            {
                if (PlayerInput.all[i].playerIndex == idx)
                    loopIndex = i;
            }
            Vector3 spawnPos = Vector3.zero;
            if (loopIndex < spawnPositionsForPlacement.Length)
                spawnPos = spawnPositionsForPlacement[loopIndex];
            Debug.Log("spawnPos = " + spawnPos);

            // 4e) Move cursor there
            cursorGO.transform.position = spawnPos;
            Debug.Log("Moved cursorGO to placement pos");

            // 4f) Re-enable visuals
            SpriteRenderer sr = cursorGO.GetComponent<SpriteRenderer>();
            Collider2D col = cursorGO.GetComponent<Collider2D>();
            Debug.Log("SpriteRenderer = " + (sr == null ? "null" : "exists") +
                      ", Collider2D = " + (col == null ? "null" : "exists"));
            if (sr != null) sr.enabled = true;
            if (col != null) col.enabled = true;
            Debug.Log("Re-enabled sprite & collider");

            // 4g) Check the prefab
            GameObject prefabToGive = kvp.Value;
            if (prefabToGive == null)
            {
                Debug.LogError("prefabToGive is NULL for playerIndex " + idx);
                continue;
            }
            Debug.Log("prefabToGive = " + prefabToGive.name);

            // 4h) Call placement method
            Debug.Log("Calling cc.BeginPlacementPhase(...)");
            try
            {
                cc.BeginPlacementPhase(prefabToGive, spawnPos);
                Debug.Log("SUCCESS: BeginPlacementPhase completed");
            }
            catch (System.Exception e)
            {
                Debug.LogError("EXCEPTION in BeginPlacementPhase: " + e);
            }
        }

        Debug.Log("<<< End of BeginPlacementPhaseAll()");
    }

    // Called by CursorController when a player finishes placing their attached object
    public void NotifyPlayerPlaced(int playerIndex)
    {
        if (!playersThatPlaced.Contains(playerIndex))
        {
            Debug.Log("player: " + playerIndex + "placed");
            playersThatPlaced.Add(playerIndex);
        }
        Debug.Log("player that placed count:" + playersThatPlaced.Count);
        Debug.Log("playercount: " + playersThatJoined);
        if (playersThatPlaced.Count == playersThatJoined)
        {
            Debug.Log("gonna call allplayerfinishedplacing");
            AllPlayersFinishedPlacing();
        }
    }

    private void AllPlayersFinishedPlacing()
    {
        Debug.Log("All players have finished placing!");
        GridPlacementSystem.Instance.HideGrid();
        // now you can move on: load next scene, unlock game logic, etc.
    }

}
*/