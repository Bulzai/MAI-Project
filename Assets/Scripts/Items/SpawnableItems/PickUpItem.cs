using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpItem : MonoBehaviour
{
    public enum ItemType { Slow, Repel, Speed, Damage, Confusion }
    public ItemType itemType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Item picked up: " + itemType);

        PlayerInput playerInput = other.GetComponentInParent<PlayerInput>();

        other.GetComponent<PlayerItemHandler>()?.ApplyItem(itemType);

        if (playerInput != null && GameMetricsLogger.Instance != null)
        {
            GameMetricsLogger.Instance.RegisterAuraCollected(playerInput.playerIndex, itemType);
        }

        Destroy(gameObject);
    }
}