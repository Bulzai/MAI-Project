using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickUpItem : MonoBehaviour
{
    public enum ItemType { Slow, Repel, Speed, Damage, Confusion }
    public ItemType itemType;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Item picked up: " + itemType);

            PlayerInput playerInput = other.GetComponentInParent<PlayerInput>();

            // Tell the player to apply the effect
            other.GetComponent<PlayerItemHandler>().ApplyItem(itemType);

            // Track aura collection per player
            if (playerInput != null && GameMetricsLogger.Instance != null)
            {
                GameMetricsLogger.Instance.RegisterAuraCollected(playerInput.playerIndex, itemType);
            }

            // Show popup near the correct player's health bar
            if (playerInput != null && PopupTextManager.Instance != null)
            {
                string popupText = GetPopupText(itemType);
                Color popupColor = GetPopupColor(itemType);

                PopupTextManager.Instance.ShowPopupForPlayer(popupText, playerInput.playerIndex, popupColor);
            }

            // Destroy or disable the item
            Destroy(gameObject);
        }
    }

    private string GetPopupText(ItemType type)
    {
        switch (type)
        {
            case ItemType.Slow:
                return "Slow Others!";
            case ItemType.Repel:
                return "Push Others!";
            case ItemType.Speed:
                return "Fastest Now!";
            case ItemType.Confusion:
                return "Confuse Others!";
            default:
                return "Aura";
        }
    }

    private Color GetPopupColor(ItemType type)
    {
        switch (type)
        {
            case ItemType.Speed:
                return Color.white;

            case ItemType.Slow:
                return Color.cyan;

            case ItemType.Repel:
                return Color.yellow;

            case ItemType.Confusion:
                return new Color(1f, 0.4f, 0.8f); // pink/purple

            default:
                return Color.white;
        }
    }
}