using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerzoneTracker : MonoBehaviour
{
    // dictionary to map player to their danger zone time
    private Dictionary<GameObject, float[]> playersDangerzoneTime = new Dictionary<GameObject, float[]>();

    // to get current round
    private RoundController roundManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private GameObject FindPlayerRoot(Collider2D collider)
    {
        // check if collided object has the tag
        if(collider.CompareTag("Player")) return collider.transform.root.gameObject;

        if (collider.transform.root.CompareTag("Player")) return collider.transform.root.gameObject;

        return null;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (GameEvents.CurrentState != GameState.MainGameState) return;

        roundManager = Object.FindAnyObjectByType<RoundController>();

        if (roundManager == null) Debug.LogError("RoundManager was not found in scene");

        // find player root object
        GameObject playerRoot = FindPlayerRoot(collider);

        if(playerRoot != null)
        {
            if(!playersDangerzoneTime.ContainsKey(playerRoot))
            {
                float[] newEntry = new float[roundManager.getMaxRounds()];
                playersDangerzoneTime.Add(playerRoot, newEntry);
                Debug.Log($"{playerRoot.name} initialized danger zone time tracking at round {roundManager.currentRound}.");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (GameEvents.CurrentState != GameState.MainGameState) return;

        roundManager = Object.FindAnyObjectByType<RoundController>();

        if (roundManager == null) Debug.LogError("RoundManager was not found in scene");

        GameObject playerRoot = FindPlayerRoot (collider);

        // current round time
        if(playerRoot != null && playersDangerzoneTime.ContainsKey(playerRoot))
        {
            playersDangerzoneTime[playerRoot][roundManager.currentRound] += Time.deltaTime;
        }

    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (GameEvents.CurrentState != GameState.MainGameState) return;

        roundManager = Object.FindAnyObjectByType<RoundController>();

        if (roundManager == null) Debug.LogError("RoundManager was not found in scene");

        GameObject playerRoot = FindPlayerRoot(collider);

        if (playerRoot != null && playersDangerzoneTime.ContainsKey(playerRoot)) 
        {
            float currentRoundTime = playersDangerzoneTime[playerRoot][roundManager.currentRound];
            Debug.Log($"{playerRoot.name} left danger zone. Round {roundManager.currentRound} Time: {currentRoundTime:F2}s");
        }
            
    }
}
