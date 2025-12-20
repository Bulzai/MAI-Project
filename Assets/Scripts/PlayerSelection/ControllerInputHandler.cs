using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerInputHandler : MonoBehaviour
{
   /* private PlayerInput playerInput;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    // This is automatically called by Input System when Y is pressed
    public void OnReady(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log($"ControllerInputHandler: Player {playerInput.playerIndex} pressed Y");

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
            Debug.Log($"ControllerInputHandler: Player {playerInput.playerIndex} pressed A");

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
            Debug.Log($"ControllerInputHandler: Player {playerInput.playerIndex} pressed B");

            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.OnReturnPressed();
            }
        }
    }*/
}