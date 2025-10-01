using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SurpriseBoxState : MonoBehaviour
{
    public static SurpriseBoxState Instance { get; private set; }

    [SerializeField] private PlayerManager playerManager;

    public GameObject SurpriseBox;
    public GameObject SelectPlayer;
    public GameObject GameWorld;
    public GameObject PlayerSelectionButton;


    [Header("Item Stuff")]
    [SerializeField] private GameObject itemBox;
    [SerializeField] private GameObject surpriseBoxObject;
    [SerializeField] private List<GameObject> spawnBoxes;
    [SerializeField] private List<GameObject> itemPool;
    [SerializeField] private int numberToSpawn;


    private Transform itemBoxItemList;

    private List<GameObject> itemsInBox = new List<GameObject>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var go = GameObject.Find("ItemBoxItemList");
        if (go == null)
            Debug.LogError("Missing 'ItemBoxItemList' GameObject!");
        else
            itemBoxItemList = go.transform;


    }


    private void OnEnable()
    {
        GameEvents.OnSurpriseBoxStateEntered += ActivateItemBox;
        GameEvents.OnSurpriseBoxStateEntered += SpawnObjects;
        GameEvents.OnSurpriseBoxStateEntered += ShowAllCursors;

    }

    private void OnDisable()
    {
        //GameEvents.OnSurpriseBoxStateEntered -= OnPickItemStateEntered;
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnObjects()
    {
        // Copy list so we can remove used tiles
        var availableTiles = new List<GameObject>(spawnBoxes);

        for (int i = 0; i < numberToSpawn; i++)
        {
            // pick item prefab
            var prefab = itemPool[Random.Range(0, itemPool.Count)];
            var rate = prefab.GetComponent<SelectableItem>().GetSpawnRate();

            if (Random.Range(0f, 100f) > rate)
            {
                i--; // try again
                continue;
            }

            // pick a random tile and remove it from the pool
            int idx = Random.Range(0, availableTiles.Count);
            var tile = availableTiles[idx];
            availableTiles.RemoveAt(idx);

            // choose a random point inside its mesh‐bounds
            var col = tile.GetComponent<MeshCollider>();
            var x = Random.Range(col.bounds.min.x + 1, col.bounds.max.x - 1);
            var y = Random.Range(col.bounds.min.y + 1, col.bounds.max.y - 1);
            var pos = new Vector2(x, y);

            // instantiate
            var go = Instantiate(prefab, pos, prefab.transform.rotation, itemBoxItemList);
            itemsInBox.Add(go);

        }
    }
    private void ActivateItemBox()
    {
        itemBox.SetActive(true);

    }

    public void DeactivateItemBox()
    {
        itemBox.SetActive(false);
        surpriseBoxObject.SetActive(false);
        // Destroy all spawned items
        foreach (var go in itemsInBox)
            if (go != null)
                Destroy(go);

        itemsInBox.Clear();

    }



    // Called by CursorController when a player picks an item
    public void NotifyPlayerPicked(int idx, GameObject prefab)
    {

        if (!playerManager.pickedPrefabByPlayer.ContainsKey(idx))
        {
            playerManager.pickedPrefabByPlayer[idx] = prefab;
            var cursor = playerManager.playerRoots[idx].transform.Find("CursorNoPI").gameObject;
            cursor.SetActive(false);

        }


        if (playerManager.pickedPrefabByPlayer.Count == playerManager.playerCount)
        {
            Debug.Log("All picked → BeginPlacementPhaseAll");
            DeactivateItemBox();
            GameEvents.ChangeState(GameState.PlaceItemState);
            //BeginPlacementPhaseAll();
        }
    }

    public void ShowAllCursors()
    {
        foreach (var kvp in playerManager.playerRoots)
        {
            int idx = kvp.Key;
            var root = kvp.Value;
            var pi = root.GetComponent<PlayerInput>();
            var cursor = root.transform.Find("CursorNoPI").gameObject;

            // Reset cursor position using PlayerManager
            playerManager.ResetCursorPositionItemSelection(idx);

            // 1) turn the cursor graphic on
            cursor.SetActive(true);

            // 2) swap to the Cursor map
            pi.SwitchCurrentActionMap("Cursor");

            // 3) debug post‐switch
          //Debug.Log($"[POST‐SWITCH] {pi.playerIndex} → map={pi.currentActionMap.name}");
          //Debug.Log($"[POST‐SWITCH] {pi.playerIndex} → map={pi.currentActionMap.name}");
          //foreach (var a in pi.currentActionMap)
          //    Debug.Log($"    {a.name} enabled={a.enabled}");
        }
    }



    // is called in playerselection, but should be called here?
    private void InitializeSurprisebox()
    {
        GameWorld.SetActive(false);
        SelectPlayer.SetActive(false);
        SurpriseBox.SetActive(true);
        PlayerSelectionButton.SetActive(false);
    }
}
