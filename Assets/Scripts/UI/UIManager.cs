using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    //public GameObject ItemSelectionPanel;
    //public GridPlacementSystem placementSystem;

    public GameObject returnToMainMenuButtonAfterFinalScreen;

    public GameObject itemsParent;


    public GameObject extinguisherParent;
    // [SerializeField] private GameObject grid;


    private void Start()
    {

        GameEvents.OnFinalScoreStateEntered += ActivateReturnToMainMenuButton;
    }

    void ActivateReturnToMainMenuButton()
    {
        returnToMainMenuButtonAfterFinalScreen.SetActive(true);
    }
    /*public void ToggleItemSelectionPanel()
    {
        ItemSelectionPanel.SetActive(!ItemSelectionPanel.activeSelf);
    }

    public void Toggle()
    {
        grid.SetActive(!grid.activeSelf);
    }*/

    private void OnEnable()
    {

        GameEvents.OnFinalScoreStateEntered += ActivateReturnToMainMenuButton;
        //GameEvents.OnItemSelectionPanelOpened += ToggleItemSelectionPanel;
        // GameEvents.OnToggleGrid += Toggle;
    }

    private void OnDisable()
    {

        GameEvents.OnFinalScoreStateEntered -= ActivateReturnToMainMenuButton;

        //GameEvents.OnItemSelectionPanelOpened -= ToggleItemSelectionPanel;
        //GameEvents.OnToggleGrid -= Toggle;
    }



    public void ResetGame()
    {


        StopAllCoroutines();
        GameEvents.ChangeState(GameState.MenuState);
        Time.timeScale = 1f;
        //delete all items



        PlayerManager.Instance.HardResetGame();

        foreach (var psm in FindObjectsOfType<PlayerScoreManager>())
        {
            psm.ClearCaches();
        }

        DeleteItems();
    }


    void DeleteItems()
    {
        foreach (Transform child in itemsParent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in extinguisherParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

 
}
