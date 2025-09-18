using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    public enum ItemType { Slow, Explosion }  
    public ItemType itemType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            Debug.Log("Item picked up: " + itemType);
            // Tell the player to apply the effect
            other.GetComponent<PlayerItemHandler>().ApplyItem(itemType);

            // Destroy or disable the item
            Destroy(gameObject);
        }
    }
}
