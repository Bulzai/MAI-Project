using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SurpriseBoxState : MonoBehaviour
{
    public static SurpriseBoxState Instance { get; private set; }

    public static event Action<GameObject> OnPlayerPickedItem;
    public static event Action OnFireTransitionAnimationStarted;

    [SerializeField] private PlayerManager playerManager;

    public GameObject SurpriseBox;
    public GameObject SelectPlayer;
    public GameObject GameWorld;
    public GameObject PlayerSelectionButton;

    [Header("Item Stuff")]
    [SerializeField] private List<GameObject> spawnBoxes;
    [SerializeField] private List<GameObject> itemPool;
    [SerializeField] private int numberToSpawn;

    public GameObject[] playerNamesToDeactive;

    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private string playAnimTrigger = "Play";

    [SerializeField] private Transform itemBoxItemList;

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

        PlayerSelectionManager.OnReturnToMainMenu += DeactivePlayerNames;
    }

    private void OnDestroy()
    {
        PlayerSelectionManager.OnReturnToMainMenu -= DeactivePlayerNames;
    }

    private void OnEnable()
    {
        GameEvents.OnSurpriseBoxStateEntered += ActivateItemBox;
        GameEvents.OnSurpriseBoxStateEntered += SpawnObjects;
        GameEvents.OnSurpriseBoxStateEntered += DeactivePlayerNames;
        GameEvents.OnSurpriseBoxStateEntered += ShowAllCursors;
    }

    private void OnDisable()
    {
        GameEvents.OnSurpriseBoxStateEntered -= ActivateItemBox;
        GameEvents.OnSurpriseBoxStateEntered -= SpawnObjects;
        GameEvents.OnSurpriseBoxStateEntered -= DeactivePlayerNames;
        GameEvents.OnSurpriseBoxStateEntered -= ShowAllCursors;
    }

    public void DeactivePlayerNames()
    {
        if (playerNamesToDeactive == null) return;

        foreach (var name in playerNamesToDeactive)
        {
            if (name != null)
                name.SetActive(false);
        }
    }

    public void SpawnObjects()
    {
        var availableTiles = new List<GameObject>(spawnBoxes);

        if (itemPool == null || itemPool.Count == 0 || availableTiles.Count == 0)
            return;

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

            if (prefab == null)
                continue;

            var selectableItem = prefab.GetComponent<SelectableItem>();
            if (selectableItem == null)
            {
                Debug.LogWarning($"Prefab {prefab.name} has no SelectableItem component.");
                continue;
            }

            float rate = selectableItem.GetSpawnRate();

            if (UnityEngine.Random.Range(0f, 100f) > rate)
            {
                i--;
                continue;
            }

            usedIndices.Add(prefabIndex);

            int idx = UnityEngine.Random.Range(0, availableTiles.Count);
            var tile = availableTiles[idx];
            availableTiles.RemoveAt(idx);

            if (tile == null)
                continue;

            var col = tile.GetComponent<MeshCollider>();
            Vector2 pos;

            if (col != null)
            {
                Vector3 center = col.bounds.center;
                pos = new Vector2(center.x, center.y);
            }
            else
            {
                pos = tile.transform.position;
            }

            GameObject go;

            if (itemBoxItemList != null)
                go = Instantiate(prefab, pos, prefab.transform.rotation, itemBoxItemList);
            else
                go = Instantiate(prefab, pos, prefab.transform.rotation);

            itemsInBox.Add(go);
        }
    }

    private void ActivateItemBox()
    {
        if (SurpriseBox != null)
            SurpriseBox.SetActive(true);
    }

    public void DeactivateItemBox()
    {
        if (SurpriseBox != null)
            SurpriseBox.SetActive(false);

        foreach (var go in itemsInBox)
        {
            if (go != null)
                Destroy(go);
        }

        itemsInBox.Clear();
    }

    public void NotifyPlayerPicked(int idx, GameObject prefab)
    {
        if (playerManager == null)
        {
            Debug.LogWarning("PlayerManager is missing.");
            return;
        }

        if (!playerManager.pickedPrefabByPlayer.ContainsKey(idx))
        {
            playerManager.pickedPrefabByPlayer[idx] = prefab;
            OnPlayerPickedItem?.Invoke(prefab);

            if (playerManager.playerRoots.ContainsKey(idx))
            {
                var cursorTransform = playerManager.playerRoots[idx].transform.Find("CursorNoPI");
                if (cursorTransform != null)
                    cursorTransform.gameObject.SetActive(false);
            }
        }

        if (playerManager.pickedPrefabByPlayer.Count == playerManager.playerCount)
        {
            StartCoroutine(ExecuteTransitionThenChangeState());
        }
    }

    private IEnumerator ExecuteTransitionThenChangeState()
    {
        yield return new WaitForSeconds(1.0f);

        OnFireTransitionAnimationStarted?.Invoke();

        if (transitionAnimator != null)
        {
            var image = transitionAnimator.GetComponent<Image>();
            if (image != null)
                image.enabled = true;

            transitionAnimator.SetTrigger(playAnimTrigger);
        }

        yield return new WaitForSeconds(1f);

        DeactivateItemBox();
        GameEvents.ChangeState(GameState.PlaceItemState);

        yield return new WaitForSeconds(0.5f);

        if (transitionAnimator != null)
        {
            var image = transitionAnimator.GetComponent<Image>();
            if (image != null)
                image.enabled = false;
        }
    }

    public void ShowAllCursors()
    {
        if (playerManager == null || playerManager.playerRoots == null)
        {
            Debug.LogWarning("PlayerManager or playerRoots is missing.");
            return;
        }

        foreach (var kvp in playerManager.playerRoots)
        {
            int idx = kvp.Key;
            var root = kvp.Value;

            if (root == null)
                continue;

            var pi = root.GetComponent<PlayerInput>();
            var cursorTransform = root.transform.Find("CursorNoPI");

            if (playerManager != null)
                playerManager.ResetCursorPositionItemSelection(idx);

            if (cursorTransform != null)
                cursorTransform.gameObject.SetActive(true);

            if (pi != null)
                pi.SwitchCurrentActionMap("Cursor");
        }
    }
}