using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SurpriseBoxState : MonoBehaviour
{
    public static SurpriseBoxState Instance { get; private set; }

    [SerializeField] private PlayerManagerFinal playerManagerFinal;

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
        Debug.Log("SurpriseBoxState.Awake -> setting Instance");
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
            var rate = prefab.GetComponent<Itembox_Selectable>().GetSpawnRate();

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
            var go = Instantiate(prefab, pos, prefab.transform.rotation);
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
        GameEvents.ChangeState(GameState.MainGameState);

    }



    // Called by CursorController when a player picks an item
    public void NotifyPlayerPicked(int idx, GameObject prefab)
    {
        Debug.Log("NotifyPlayerPicked idx=" + idx + " prefab=" + prefab.name);
        if (!playerManagerFinal.pickedPrefabByPlayer.ContainsKey(idx))
            playerManagerFinal.pickedPrefabByPlayer[idx] = prefab;

        if (playerManagerFinal.pickedPrefabByPlayer.Count == playerManagerFinal.PlayerCount)
        {
            Debug.Log("All picked → BeginPlacementPhaseAll");
            DeactivateItemBox();
            GameEvents.ChangeState(GameState.PlaceItemState);
            //BeginPlacementPhaseAll();
        }
    }

    // Reactivate every cursor
    public void ShowAllCursors()
    {
        Debug.Log("ShowAllCursors called");
        foreach (var root in playerManagerFinal.playerRoots.Values)
        {
            Debug.Log("inputs:" + root.GetComponent<PlayerInput>());
            var cursor = root.transform.Find("CursorNoPIFinal").gameObject;
            cursor.SetActive(true);
            root.GetComponent<PlayerInput>().SwitchCurrentActionMap("Cursor");
            root.GetComponent<PlayerInput>().ActivateInput();
            Debug.Log("input activated and action map switched to cursor");


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
