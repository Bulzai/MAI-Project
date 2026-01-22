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
    public static event Action OnSurpriseBoxStateCounterStarted;
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

    [SerializeField] private TMP_Text countdownText;

    private Transform itemBoxItemList;
    private List<GameObject> itemsInBox = new List<GameObject>();

    public GameObject[] playerNamesToDeactive;

    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private string playAnimTrigger = "Play";

    private Coroutine countdownRoutine;

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
        //GameEvents.OnSurpriseBoxStateEntered += ShowAllCursors;
        GameEvents.OnSurpriseBoxStateEntered += DeactivePlayerNames;

        GameEvents.OnSurpriseBoxStateEntered += StartEnterCountdown;
    }

    private void OnDisable()
    {
        GameEvents.OnSurpriseBoxStateEntered -= ActivateItemBox;
        GameEvents.OnSurpriseBoxStateEntered -= SpawnObjects;
        GameEvents.OnSurpriseBoxStateEntered -= ShowAllCursors;
        GameEvents.OnSurpriseBoxStateEntered -= DeactivePlayerNames;
        GameEvents.OnSurpriseBoxStateEntered -= StartEnterCountdown;

        // optional safety
        StopCountdownIfRunning();
    }

    private void StopCountdownIfRunning()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
    }

    private void StartEnterCountdown()
    {
        StopCountdownIfRunning();

        // Countdown beim Betreten -> danach Countdown-Objekt ausblenden
        countdownRoutine = StartCoroutine(PlayCountdown( () =>
        {
            // Nach dem Enter-Countdown:
            // - CountdownText wird in PlayCountdown deaktiviert
            // - Hier kannst du optional Sachen triggern
            // z.B. SurpriseBox offen lassen oder nochmal extra UI aktivieren.
        }));
    }

    public IEnumerator PlayCountdown( Action onFinished, int seconds = 2, float timing = 0.6f)
    {
        yield return new WaitForSeconds(0.7f);
        OnSurpriseBoxStateCounterStarted?.Invoke();
        float countdown = seconds;

        countdownText.gameObject.SetActive(true);

        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(timing);
            countdown--;
        }

        countdownText.gameObject.SetActive(false);

        ShowAllCursors();

        countdownRoutine = null;
        onFinished?.Invoke();
    }

    public void DeactivePlayerNames()
    {
        foreach (var name in playerNamesToDeactive)
            name.SetActive(false);
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

            if (UnityEngine.Random.Range(0f, 100f) > rate)
            {
                i--;
                continue;
            }

            usedIndices.Add(prefabIndex);

            int idx = UnityEngine.Random.Range(0, availableTiles.Count);
            var tile = availableTiles[idx];
            availableTiles.RemoveAt(idx);

            var col = tile.GetComponent<MeshCollider>();
            Vector2 pos;

            if (col != null)
            {
                // Use the center of the collider's bounds
                Vector3 center = col.bounds.center;   // world space
                pos = new Vector2(center.x, center.y);
            }
            else
            {
                pos = tile.transform.position;
            }
            var go = Instantiate(prefab, pos, prefab.transform.rotation, itemBoxItemList);
            itemsInBox.Add(go);
        }
    }

    private void ActivateItemBox()
    {
        SurpriseBox.SetActive(true);
    }

    public void DeactivateItemBox()
    {
        SurpriseBox.SetActive(false);

        foreach (var go in itemsInBox)
            if (go != null)
                Destroy(go);

        itemsInBox.Clear();
    }

    public void NotifyPlayerPicked(int idx, GameObject prefab)
    {
        if (!playerManager.pickedPrefabByPlayer.ContainsKey(idx))
        {
            playerManager.pickedPrefabByPlayer[idx] = prefab;
            OnPlayerPickedItem?.Invoke(prefab);
            var cursor = playerManager.playerRoots[idx].transform.Find("CursorNoPI").gameObject;
            cursor.SetActive(false);
        }

        if (playerManager.pickedPrefabByPlayer.Count == playerManager.playerCount)
        {
            StopCountdownIfRunning();

            //OnSurpriseBoxStateCounterStarted?.Invoke();
            /*countdownRoutine = StartCoroutine(PlayCountdown(() =>
            {
                StartCoroutine(ExecuteTransitionThenChangeState());
            }));*/

            StartCoroutine(ExecuteTransitionThenChangeState());

        }
    }

  
    private IEnumerator ExecuteTransitionThenChangeState()
    {
        yield return new WaitForSeconds(1.0f);

        OnFireTransitionAnimationStarted?.Invoke();
        // 1. Das Parent-Objekt finden und aktivieren
        transitionAnimator.gameObject.GetComponent<Image>().enabled = true;

        // 2. Animation Trigger setzen
        transitionAnimator.SetTrigger("Play");

        yield return new WaitForSeconds(1f);

        // 6. Die Spiellogik ausführen
        DeactivateItemBox();
        GameEvents.ChangeState(GameState.PlaceItemState);

        yield return new WaitForSeconds(0.5f);
        transitionAnimator.gameObject.GetComponent<Image>().enabled = false;

    }
    public void ShowAllCursors()
    {
        foreach (var kvp in playerManager.playerRoots)
        {
            int idx = kvp.Key;
            var root = kvp.Value;
            var pi = root.GetComponent<PlayerInput>();
            var cursor = root.transform.Find("CursorNoPI").gameObject;

            playerManager.ResetCursorPositionItemSelection(idx);

            cursor.SetActive(true);
            pi.SwitchCurrentActionMap("Cursor");
        }
    }
}
