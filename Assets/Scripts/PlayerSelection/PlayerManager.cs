using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;


public class PlayerManager : MonoBehaviour
{
    public Transform[] SpawnPoints;

    public List<PlayerInput> players = new List<PlayerInput>();
    private int PlayerCount;

    // ----------- WORK IN PROGRESS
    private Dictionary<string, GameObject> PlayerItems;
    // -----------


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.position = SpawnPoints[PlayerCount].transform.position;
        PlayerCount++;
        players.Add(playerInput);

        int playerIndex = playerInput.playerIndex;
        playerInput.gameObject.name = $"Player {playerIndex + 1}";

        Debug.Log($"Player joined haha: {playerInput.gameObject.name}");

        // ----------- WORK IN PROGRESS
        // Subscribe to the click event on this player's actions
        playerInput.actions["Choose Item"].performed += context => OnClick(playerInput, context);
        // -----------

        //playerInput.DeactivateInput();
    }

    public void DeactivePlayerPrefab()
    {
        foreach (PlayerInput p in players)
        {
            p.gameObject.SetActive(false);
        }
    }

    public void ActivatePlayerPrefab()
    {
        foreach (PlayerInput p in players.ToList())
        {
            p.gameObject.SetActive(true);
            p.ActivateInput();
        }
    }

    // ----------- WORK IN PROGRESS
    private void OnClick(PlayerInput playerInput, InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var clickedItem = hit.collider.gameObject;
            // Now you know playerInput (the player) and the clickedItem
            Debug.Log($"Player {playerInput.playerIndex} clicked {clickedItem.name}");
        }
    }
    // -----------
}
