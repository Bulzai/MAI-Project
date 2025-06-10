using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class UnpairMouseKeyboard : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoined;
    }

    private void OnDisable()
    {
        PlayerInputManager.instance.onPlayerJoined -= HandlePlayerJoined;
    }

    private void HandlePlayerJoined(PlayerInput player)
    {
        var user = player.user;

        // Unpair mouse and keyboard from this player
        foreach (var device in user.pairedDevices)
        {
            if (device is Mouse || device is Keyboard)
            {
                Debug.Log($"Unpairing {device.displayName} from Player {player.playerIndex}");
                user.UnpairDevice(device);
            }
        }

        // Ensure they aren't auto-switched again
        player.neverAutoSwitchControlSchemes = true;
    }
}
