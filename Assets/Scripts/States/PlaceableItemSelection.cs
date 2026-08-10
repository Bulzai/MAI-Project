using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlaceableItemSelection : MonoBehaviour
{
    public static PlaceableItemSelection Instance { get; private set; }

    [SerializeField] public GameObject selectionManagerGO;
    private PlayerSelectionManager selectionManager;

    [Header("Item Pool")]
    [SerializeField] private List<GameObject> generalItemPool;

    [Header("UI Feedback")]
    public Button playButton;
    public GameObject readyWarningText;

    [SerializeField] public GameObject warningText;
    [SerializeField] private float warningDuration = 2f;
    private Coroutine _warningCoroutine;

    [SerializeField] private GameObject[] playerReadyGO;
    [SerializeField] private GameObject[] playerUnreadyGO;

    // items surprise box will spawn
    public List<GameObject> activeItemPool { get; private set; } = new List<GameObject>();

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // default setting: all items are selected
        activeItemPool = new List<GameObject>(generalItemPool);

        selectionManager = selectionManagerGO.GetComponent<PlayerSelectionManager>();
        selectionManager.ResetAllPlayersReadyStatus();
        ShowPlayerSlots();
    }

    private void OnDisable()
    {
        selectionManager.ResetAllPlayersReadyStatus();
    }

    // GETTER
    public List<GameObject> getGeneralItemPool()
    {
        return generalItemPool;
    }

    // MAIN LOGIC
    public void ToggleItemAvailability(GameObject itemPrefab, bool isSelected)
    {
        if (!isSelected && activeItemPool.Count <= 5)
        {
            LastItemWarning();
            Debug.LogWarning("Cannot deselect the last item! The Surprise Box needs at least one thing to spawn.");
            return;
        }

        if (isSelected && !activeItemPool.Contains(itemPrefab))
        {
            activeItemPool.Add(itemPrefab);
        }
        else if (!isSelected && activeItemPool.Contains(itemPrefab))
        { 
            activeItemPool.Remove(itemPrefab);
        }
    }
    private void LastItemWarning()
    {
        if (warningText == null) return;

        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
        _warningCoroutine = StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {
        warningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        warningText.gameObject.SetActive(false);
    }
  
    // READY OR NOT
    private void ShowPlayerSlots()
    {
        for (int i = 0; i < 4; i++)
        {
            bool joined = selectionManager.isPlayerJoined(i);

            // all are unready
            if (i < playerReadyGO.Length && playerReadyGO[i] != null)
                playerReadyGO[i].SetActive(false);

            if (i < playerUnreadyGO.Length && playerUnreadyGO[i] != null)
                playerUnreadyGO[i].SetActive(joined);
        }
    }

    public void UpdateReadyStatus(PlayerInput playerInput, PlayerSelectionData data)
    {
        int index = playerInput.playerIndex;

        Debug.Log($"{index} player index is {data.IsReady}");

        if (index >= 0 && index < playerReadyGO.Length && index < playerUnreadyGO.Length)
        {
            // turn on ready and off unready if IsReady is true
            // turn off ready and on unready if IsReady is false
            if (playerReadyGO[index] != null)
                playerReadyGO[index].SetActive(data.IsReady);

            if (playerUnreadyGO[index] != null)
                playerUnreadyGO[index].SetActive(!data.IsReady);
        }
        else
        {
            Debug.LogWarning($"Player Index {index} is out of range for the UI arrays!");
        }
    }
}
