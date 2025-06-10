using UnityEngine;
using UnityEngine.InputSystem;

public class CustomVirtualMouse : MonoBehaviour
{
    public PlayerInput playerInput; // Assigned externally
    public RectTransform cursorTransform;
    public float moveSpeed = 1000f;

    private InputAction moveAction;
    private Vector2 screenPosition;

    private void Start()
    {
        if (playerInput == null)
        {
            Debug.LogError("CustomVirtualMouse requires a PlayerInput reference.");
            enabled = false;
            return;
        }

        moveAction = playerInput.actions["Move"];
        screenPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    private void Update()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        screenPosition += input * moveSpeed * Time.deltaTime;
        screenPosition = new Vector2(
            Mathf.Clamp(screenPosition.x, 0, Screen.width),
            Mathf.Clamp(screenPosition.y, 0, Screen.height)
        );

        Vector2 canvasPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorTransform.parent as RectTransform,
            screenPosition,
            null,
            out canvasPos
        );
        cursorTransform.anchoredPosition = canvasPos;
    }
}
