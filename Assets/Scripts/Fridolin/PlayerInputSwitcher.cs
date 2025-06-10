using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerInputSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject virtualMousePrefab; // Your cursor prefab (e.g. gingerbread tree)
    [SerializeField] private Canvas uiCanvas;               // Canvas to hold all cursors

    private bool isInUIMode = false;

    // Store each player's cursor instance
    private Dictionary<PlayerInput, GameObject> playerCursors = new();

    public void ToggleInputMode()
    {
        var allPlayers = FindObjectsOfType<PlayerInput>();
        Debug.Log("Toggling input mode...");

        foreach (var player in allPlayers)
        {
            if (!isInUIMode)
            {
                player.SwitchCurrentActionMap("UI");

                // Only spawn cursor if we haven’t yet for this player
                if (!playerCursors.ContainsKey(player))
                {
                    GameObject vm = Instantiate(virtualMousePrefab, uiCanvas.transform);
                    vm.name = $"VirtualCursor_{player.playerIndex}";
                    vm.transform.SetAsLastSibling();

                    // Optional: start cursor centered
                    RectTransform vmRect = vm.GetComponent<RectTransform>();
                    if (vmRect != null)
                        vmRect.anchoredPosition = Vector2.zero;

                    // Pair input
                    InputUser user = InputUser.CreateUserWithoutPairedDevices();
                    user.AssociateActionsWithUser(player.actions);
                    if (player.devices.Count > 0)
                        InputUser.PerformPairingWithDevice(player.devices[0], user);

                    var vmi = vm.GetComponent<UnityEngine.InputSystem.UI.VirtualMouseInput>();
                    if (vmi != null)
                    {
                        InputUser.PerformPairingWithDevice(player.devices[0], user);
                    }


                    // Track cursor
                    playerCursors[player] = vm;
                }
            }
            else
            {
                player.SwitchCurrentActionMap("Player");

                if (playerCursors.TryGetValue(player, out GameObject cursor))
                {
                    Destroy(cursor);
                    playerCursors.Remove(player);
                }
            }
        }

        isInUIMode = !isInUIMode;
    }
}
