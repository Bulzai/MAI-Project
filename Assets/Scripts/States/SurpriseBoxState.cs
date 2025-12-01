using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [SerializeField] private List<GameObject> spawnBoxes;
    [SerializeField] private List<GameObject> itemPool;
    [SerializeField] private int numberToSpawn;

    [SerializeField] private TMP_Text countdownText;

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




    public void SpawnObjects()
    {
        var availableTiles = new List<GameObject>(spawnBoxes);
        if (itemPool == null || itemPool.Count == 0 || availableTiles.Count == 0) return;

        HashSet<int> usedIndices = new HashSet<int>();

        for (int i = 0; i < numberToSpawn && availableTiles.Count > 0; i++)
        {
            if (usedIndices.Count >= itemPool.Count)
            {
                Debug.LogWarning("All unique items used — cannot spawn more without duplicates.");
                break;
            }

            int prefabIndex;
            int attempts = 0;

            // find an unused index
            do
            {
                prefabIndex = UnityEngine.Random.Range(0, itemPool.Count);
                attempts++;
                if (attempts > 50)
                {
                    Debug.LogWarning("Could not find a valid unused item after 50 attempts.");
                    return;
                }
            }
            while (usedIndices.Contains(prefabIndex));

            var prefab = itemPool[prefabIndex];
            float rate = prefab.GetComponent<SelectableItem>().GetSpawnRate();

            // roll spawn rate
            if (UnityEngine.Random.Range(0f, 100f) > rate)
            {
                i--; // try again for this slot (do NOT mark used)
                continue;
            }

            // ✅ mark this item as used so it can't spawn again this wave
            usedIndices.Add(prefabIndex);

            // pick a unique tile
            int idx = UnityEngine.Random.Range(0, availableTiles.Count);
            var tile = availableTiles[idx];
            availableTiles.RemoveAt(idx);

            // position inside bounds (fallback if no MeshCollider)
            var col = tile.GetComponent<MeshCollider>();
            Vector2 pos = (col != null)
                ? new Vector2(
                    UnityEngine.Random.Range(col.bounds.min.x + 1f, col.bounds.max.x - 1f),
                    UnityEngine.Random.Range(col.bounds.min.y + 1f, col.bounds.max.y - 1f))
                : (Vector2)tile.transform.position;

            var go = Instantiate(prefab, pos, prefab.transform.rotation, itemBoxItemList);
            itemsInBox.Add(go);
        }
    }

    private void ActivateItemBox()
    {
        //itemBox.SetActive(true);
        SurpriseBox.SetActive(true);
    }

    public void DeactivateItemBox()
    {
        SurpriseBox.SetActive(false);
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
            StartCoroutine(CountdownBeforeMainGame());

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


        }
    }

    private IEnumerator CountdownBeforeMainGame()
    {
        int countdown = 3;  

        while (countdown > 0)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = countdown.ToString();




            yield return new WaitForSeconds(1f);
            countdown--;
        }

       
        countdownText.gameObject.SetActive(false);
        DeactivateItemBox();
        GameEvents.ChangeState(GameState.PlaceItemState);

    }


}
