using System;
using TarodevController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUi;
   
    public static event Action OnPauseSFXEvent;
    public static event Action OnResumeSFXEvent;
    private void Awake()
    {
        PausePlayerRoot.OnPauseEvent += OnPauseEvent;
    }
    
    private void OnDestroy()
    {
        PausePlayerRoot.OnPauseEvent -= OnPauseEvent;
    }
    
    public void Resume()
    {
        pauseMenuUi.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    public void Pause()
    {
        Debug.Log("Pause is called in pausemenu");
        pauseMenuUi.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;

        // Find first selectable UI element inside the pause menu to navigate with the controller
        Selectable firstSelectable = pauseMenuUi.GetComponentInChildren<Selectable>();
        if (firstSelectable != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectable.gameObject);
        }
    }

    private void TogglePause()
    {
        if (gameIsPaused)
        {
            Resume();
            OnResumeSFXEvent?.Invoke();
        } else
        {
            Pause();
            OnPauseSFXEvent?.Invoke();
        }
    }


    public void OnPauseEvent()
    {
        
        if (GameEvents.CurrentState != GameState.MainGameState)
            return;
        TogglePause();
    }

}
