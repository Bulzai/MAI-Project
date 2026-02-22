using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPools : MonoBehaviour
{   
    [SerializeField] private List<ItemEntry> itemEntries;
    private Dictionary<ItemType, GameObject> itemDictionary;
    [SerializeField] private List<GameObject> itemPool;

    
    public static ItemPools Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        
        itemDictionary = new Dictionary<ItemType, GameObject>();

        foreach (var entry in itemEntries)
        {
            if (!itemDictionary.ContainsKey(entry.key))
            {
                itemDictionary.Add(entry.key, entry.value);
            }
            else
            {
                Debug.LogWarning($"Duplicate key: {entry.key}");
            }
        }
    }

    public GameObject GetItemFromKey(ItemType type)
    {
        return itemDictionary[type];
    }

    public GameObject GetRandomItem()
    {
        if (itemPool.Count == 0)
        {
            Debug.LogWarning("Item pool is empty!");
            return null;
        }

        int randomIndex = Random.Range(0, itemPool.Count);
        return itemPool[randomIndex];
    }
    
}

[System.Serializable]
public class ItemEntry
{
    public ItemType key;
    public GameObject value;
}

public enum ItemType
{
    Effect_Shooter,
    Spike,
    Candle,
    FlameBurner,
    cane
}