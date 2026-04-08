using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlaceableItemToggleUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponentInChildren<Toggle>();
        _toggle.onValueChanged.AddListener(OnToggleClicked);
    }

    public void SetupToggle(GameObject prefab)
    {
        itemPrefab= prefab;
        _toggle = GetComponentInChildren<Toggle>();

        // check if item already in pool
        if (PlaceableItemSelection.Instance != null) 
        {
            _toggle.SetIsOnWithoutNotify(PlaceableItemSelection.Instance.activeItemPool.Contains(itemPrefab));
        }
    }

    void OnToggleClicked(bool isOn)
    {
        PlaceableItemSelection.Instance.ToggleItemAvailability(itemPrefab, isOn);

        // if manager rejected change force to stay on
        _toggle.SetIsOnWithoutNotify(PlaceableItemSelection.Instance.activeItemPool.Contains(itemPrefab));
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
