using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUi;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            TogglePause();
        }
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
            Resume();
        else
            Pause();
    }
}
