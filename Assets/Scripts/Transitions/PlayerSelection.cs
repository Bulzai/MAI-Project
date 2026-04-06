using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    public GameObject PlaceableItemSelectionManager; 
    public GameObject SelectPlayer;
    public GameObject GameWorld;
    public GameObject PlayerSelectionButton;
    private void OnEnable()
    {
        GameEvents.OnPlaceableItemSelectionStateEntered += StartPlaceableItemSelectionSequence;
    }
    private void OnDisable()
    {
        GameEvents.OnPlaceableItemSelectionStateEntered -= StartPlaceableItemSelectionSequence;
    }
    public void StartPlaceableItemSelectionSequence()
    {

        GameWorld.SetActive(false);
        SelectPlayer.SetActive(false);
        PlaceableItemSelectionManager.SetActive(true);
        PlayerSelectionButton.SetActive(false);  
    }
}
