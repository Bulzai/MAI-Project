using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceableItemSelection : MonoBehaviour
{
    public static PlaceableItemSelection Instance { get; private set; }

    [Header("Item Pool")]
    [SerializeField] private List<GameObject> generalItemPool;

    [SerializeField] private SurpriseBoxState surpriseBoxState;

    [Header("UI Feedback")]
    [SerializeField] public GameObject warningText;
    [SerializeField] private float warningDuration = 2f;
    private Coroutine _warningCoroutine;

    // items surprise box will spawn
    public List<GameObject> activeItemPool { get; private set; } = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // default setting: all items are selected
        activeItemPool = new List<GameObject>(generalItemPool);
    }

    private void OnDisable()
    {
        FinishedSelection();
    }

    public void ToggleItemAvailability(GameObject itemPrefab, bool isSelected)
    {
        if (!isSelected && activeItemPool.Count <= 1)
        {
            ShowWarning("Cannot deselect the last item!");
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
    private void ShowWarning(string message)
    {
        if (warningText == null) return;

        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
        _warningCoroutine = StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningText.gameObject.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        warningText.gameObject.SetActive(false);
    }

    public void FinishedSelection()
    {
        // next up: Surprise box
        GameEvents.ChangeState(GameState.SurpriseBoxState);
    }

    public List<GameObject> getGeneralItemPool()
    {
        return generalItemPool;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
