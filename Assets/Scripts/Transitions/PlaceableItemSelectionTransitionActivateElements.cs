using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableItemSelectionTransition : MonoBehaviour
{
    public GameObject SurpriseBox;
    public GameObject SelectPlayer;
    public GameObject GameWorld;
    public GameObject PlayerSelectionButton;
    private void OnEnable()
    {
        GameEvents.OnSurpriseBoxStateEntered += StartSurpriseBoxSequence;
    }
    private void OnDisable()
    {
        GameEvents.OnSurpriseBoxStateEntered -= StartSurpriseBoxSequence;
    }
    public void StartSurpriseBoxSequence()
    {

        GameWorld.SetActive(false);
        SelectPlayer.SetActive(false);
        SurpriseBox.SetActive(true);
        PlayerSelectionButton.SetActive(false);
    }
}
