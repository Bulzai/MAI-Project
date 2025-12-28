using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class GlobalUIInput : MonoBehaviour
{
    void Awake()
    {
        var pi = GetComponent<PlayerInput>();
        pi.SwitchCurrentActionMap("UI");   // always UI
        DontDestroyOnLoad(gameObject);     // persists across scenes
    }
}