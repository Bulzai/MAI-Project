// SelectableItem.cs
// Attach this to every object in Scene that should be “picked.”

using UnityEngine;

public class SelectableItem : MonoBehaviour
{
    [Tooltip("Assign the prefab that we want to actually instantiate later (e.g. in placement).")]
    [SerializeField] private  GameObject originalPrefab;

    [HideInInspector] public bool isAvailable = true;


    public float spawnRate;

    public float GetSpawnRate()
    {
        return spawnRate;
    }


    public GameObject getOriginalPrefab()
    {
        Debug.Log("getOriginalPrefab:" + originalPrefab);
        return originalPrefab;
    }
}
