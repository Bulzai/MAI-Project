using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadZoneTracker : MonoBehaviour
{
    private Dictionary<GameObject, string> playerLocations = new Dictionary<GameObject, string>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            var zoneTrigger = child.gameObject.AddComponent<QuadZoneTrigger>();
            zoneTrigger.Initialize(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdatePlayerZone (GameObject player, string zone)
    {
        playerLocations[player] = zone;

        Debug.Log($"QUAD TRACKER: {player.name} entered {zone}.");
    }

    public void RemovePlayer(GameObject player)
    {
        if (playerLocations.ContainsKey(player)) playerLocations.Remove(player);
    }

    public string GetPlayerZone (GameObject player)
    {
         if (playerLocations.TryGetValue(player, out string zone))
        {
            return zone;
        }
        return "Unknown";
    }
}
