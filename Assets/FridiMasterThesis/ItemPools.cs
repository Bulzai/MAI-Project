using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemPools : MonoBehaviour
{   
    [SerializeField] private List<ItemEntry> itemEntries;
    private Dictionary<ItemType, GameObject> itemDictionary;
    [SerializeField] private List<GameObject> itemPool;
    [SerializeField] private List<InstantiatedItemEntry> instantiatedItemEntries;

    private Dictionary <ItemType, GameObject> instantiatedItemsDictionary;

    
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
        
        instantiatedItemsDictionary = new Dictionary<ItemType, GameObject>();
        foreach (var entry in instantiatedItemEntries)
        {
            if (!instantiatedItemsDictionary.ContainsKey(entry.key))
            {
                instantiatedItemsDictionary.Add(entry.key, entry.value);
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
            return null;
        }

        int randomIndex = Random.Range(0, itemPool.Count);
        return itemPool[randomIndex];
    }

    public GameObject GetInstantiatedItem(ItemType type)
    {
        return instantiatedItemsDictionary[type];
    }
}

[System.Serializable]
public class ItemEntry
{
    public ItemType key;
    public GameObject value;
}

[System.Serializable]
public class InstantiatedItemEntry
{
    public ItemType key;
    public GameObject value;
}


public enum ItemType
{
    none,
    Effect_Shooter,
    Spike,
    Candle,
    FlameBurner 
}
