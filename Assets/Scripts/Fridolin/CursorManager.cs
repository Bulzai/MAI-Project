using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class CursorManager : MonoBehaviour
{
    public GameObject cursorPrefab;
    public Canvas uiCanvas;

    private void OnEnable()
    {
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
    }

    private void OnDisable()
    {
        PlayerInputManager.instance.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        // Instantiate the cursor
        GameObject cursorInstance = Instantiate(cursorPrefab, uiCanvas.transform);
        cursorInstance.name = $"Cursor_{playerInput.playerIndex}";

        // Get the VirtualMouseInput component
        var virtualMouse = cursorInstance.GetComponent<VirtualMouseInput>();

        if (virtualMouse != null)
        {
            // Assign actions from the player's action map
            virtualMouse.stickAction = new InputActionProperty(playerInput.actions["Point"]);
            virtualMouse.leftButtonAction = new InputActionProperty(playerInput.actions["Click"]);
        }

        // Switch to the UI action map
        playerInput.SwitchCurrentActionMap("UI");
    }
}
