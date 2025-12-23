using UnityEngine;

public class MouseCapture : MonoBehaviour
{
    void Start()
    {
        // Hide cursor
        Cursor.visible = false;

        // Lock to center of the game window and confine it
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnDisable()
    {
        // Restore when leaving gameplay
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}