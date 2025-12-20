using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLobbyInput : MonoBehaviour
{
    /*private PlayerInput playerInput;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        // Switch to Lobby controls when in selection state
        if (GameEvents.CurrentState == GameState.PlayerSelectionState)
        {
            playerInput.SwitchCurrentActionMap("Lobby");
        }
    }

    // These methods are AUTOMATICALLY called by Input System
    public void OnReady(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Y button pressed!");

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnPlayerReadyPressed(playerInput.playerIndex);
            }
        }
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("A button pressed!");

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnStartPressed();
            }
        }
    }

    public void OnReturn(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("B button pressed!");

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnReturnPressed();
            }
        }
    }*/
}