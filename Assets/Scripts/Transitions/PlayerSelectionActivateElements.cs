using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    public GameObject PlaceableItemSelectionUI;
    public GameObject PlaceableItemSelectionBackground;
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
        PlaceableItemSelectionUI.SetActive(true);
        PlaceableItemSelectionBackground.SetActive(true);
        GameWorld.SetActive(false);
        SelectPlayer.SetActive(false);
        PlayerSelectionButton.SetActive(false);  
    }
}
