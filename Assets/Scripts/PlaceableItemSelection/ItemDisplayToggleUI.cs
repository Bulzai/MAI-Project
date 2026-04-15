//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class ItemDisplayToggleUI : MonoBehaviour
//{
//    [SerializeField] private GameObject itemPrefab;
//    private Toggle _toggle;

//    private void Awake()
//    {
//        //_toggle = GetComponentInChildren<Toggle>();
//        //_toggle.onValueChanged.AddListener(OnToggleClicked);
//    }

//    public void SetupToggle(GameObject prefab)
//    {
//        itemPrefab= prefab;
//        _toggle = GetComponentInChildren<Toggle>();

//        // check if item already in pool
//        //if (ItemDisplay.Instance != null) 
//        //{
//        //    _toggle.SetIsOnWithoutNotify(ItemDisplay.Instance.activeItemPool.Contains(itemPrefab));
//        //}
//    }

//    void OnToggleClicked(bool isOn)
//    {
//        ItemDisplay.Instance.ToggleItemAvailability(itemPrefab, isOn);

//        // if manager rejected change force to stay on
//        _toggle.SetIsOnWithoutNotify(ItemDisplay.Instance.activeItemPool.Contains(itemPrefab));
//    }
//}
