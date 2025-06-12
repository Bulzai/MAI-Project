using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnItems : MonoBehaviour
{
    [Header("Countdown Settings")]
    [SerializeField] private float startDelay   = 5f;
    [SerializeField] private TMP_Text startTimerText;
    [SerializeField] private float itemDuration = 10f;
    [SerializeField] private TMP_Text itemTimerText;

    [Header("Spawn Settings")]
    [SerializeField] private int numberToSpawn;
    [SerializeField] private List<GameObject> itemPool;
    [SerializeField] private List<GameObject> spawnBoxes;

    [Header("References")]
    [SerializeField] private GameObject itemBoxUI;
    [SerializeField] private GameObject surpriseBoxObject;
    [SerializeField] private GameObject mainGameObject;

    private List<GameObject> itemsInBox = new List<GameObject>();
    
    private void OnEnable()
    {
        OnPickItemStateEntered();
        GameEvents.OnSurpriseBoxStateEntered += OnPickItemStateEntered;
    }

    private void OnDisable()
    {
        GameEvents.OnSurpriseBoxStateEntered -= OnPickItemStateEntered;
    }

    private void OnPickItemStateEntered()
    {
        // Kick off the entire sequence when we enter PickItemState
        StartCoroutine(PickItemSequence());
    }
    
    private IEnumerator PickItemSequence()
    {
        // 1) Show start delay timer
        startTimerText.gameObject.SetActive(true);
        float t = startDelay;
        while (t > 0f)
        {
            startTimerText.text = Mathf.CeilToInt(t).ToString();
            t -= Time.deltaTime;
            yield return null;
        }
        startTimerText.gameObject.SetActive(false);
        
        // 2) Open the item box and spawn objects
        itemBoxUI.SetActive(true);
        SpawnObjects();

        // 3) Show item‐selection timer
        itemTimerText.gameObject.SetActive(true);
        t = itemDuration;
        while (t > 0f)
        {
            itemTimerText.text = "Timer: " + Mathf.CeilToInt(t).ToString();
            t -= Time.deltaTime;
            yield return null;
        }

        // 4) Clean up & transition
        itemTimerText.gameObject.SetActive(false);
        CloseItemBox();
    }

    private void SpawnObjects()
    {
        // Copy list so we can remove used tiles
        var availableTiles = new List<GameObject>(spawnBoxes);

        for (int i = 0; i < numberToSpawn; i++)
        {
            // pick item prefab
            var prefab = itemPool[Random.Range(0, itemPool.Count)];
            var rate   = prefab.GetComponent<Itembox_Selectable>().GetSpawnRate();

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
            var x   = Random.Range(col.bounds.min.x + 1, col.bounds.max.x - 1);
            var y   = Random.Range(col.bounds.min.y + 1, col.bounds.max.y - 1);
            var pos = new Vector2(x, y);

            // instantiate & schedule destroy
            var go = Instantiate(prefab, pos, prefab.transform.rotation);
            itemsInBox.Add(go);
            Destroy(go, itemDuration - 1f);
        }
    }

    private void CloseItemBox()
    {
        // hide UI
        itemBoxUI.SetActive(false);
        surpriseBoxObject.SetActive(false);
        mainGameObject.SetActive(true);
        
        
        // activate players, etc.
        /*var pm = GameObject.FindGameObjectWithTag("PlayerManager")
            .GetComponent<PlayerManager>();
        pm.ActivatePlayerPrefab();*/
        
        // CHANGE THIS BACK TO PLACEITEMSTATE!!!
        GameEvents.ChangeState(GameState.MainGameState);
        
    }
}
