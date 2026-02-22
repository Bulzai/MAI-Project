using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject MenuGameObject;
    public GameObject SelectPlayer;

    // Start is called before the first frame update
    void Start()
    {
        //Time.timeScale = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {

        MenuGameObject.SetActive(false);
        if (SelectPlayer != null)
            SelectPlayer.SetActive(true);
        Time.timeScale = 1f;
        //SceneManager.LoadScene("MovementScene");
    }
    private IEnumerator PauseBeforeQuitting()
    {
        yield return new WaitForSeconds(0.4f);
        Debug.Log("Quit");
        Application.Quit();
    }
    public void QuitGame()
    {
        StartCoroutine(PauseBeforeQuitting());
    }
}
