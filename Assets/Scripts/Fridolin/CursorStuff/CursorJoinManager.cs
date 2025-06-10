using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Linq;

public class CursorJoinManager : MonoBehaviour
{
    public GameObject cursorPrefab;
    public Canvas canvas;
    public InputActionAsset inputActions;
    private int playerCount = 0;

    private void Update()
    {
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame && (!PlayerInput.all.Any(p => p.devices.Contains(gamepad))))
            {
                SpawnCursorFor(gamepad);
            }
        }
    }

    void SpawnCursorFor(Gamepad gamepad)
    {
        var go = new GameObject($"Player_{playerCount}");
        var playerInput = go.AddComponent<PlayerInput>();
        playerInput.actions = Instantiate(inputActions);
        playerInput.defaultActionMap = "Cursor";
        playerInput.neverAutoSwitchControlSchemes = true;
        InputUser.PerformPairingWithDevice(gamepad, playerInput.user);

        var cursor = Instantiate(cursorPrefab, canvas.transform);
        cursor.name = $"Cursor_{playerCount}";

        var customMouse = cursor.GetComponent<CustomVirtualMouse>();
        customMouse.playerInput = playerInput;
        customMouse.cursorTransform = cursor.GetComponent<RectTransform>();

        playerCount++;
    }
}
