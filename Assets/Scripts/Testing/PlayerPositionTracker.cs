using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPositionTracker : MonoBehaviour
{
    private string sessionID;
    private float time;
    private float logTick = 2f;


    // Start is called before the first frame update
    void Start()
    {
        sessionID = TestingManager.Instance.GetSessionID();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameEvents.CurrentState != GameState.MainGameState) return;

        time += Time.deltaTime;

        if (time > logTick)
        {
            RecordPlayerPosition();
            time = 0;
        }
    }

    private void RecordPlayerPosition()
    {
        int round = Object.FindAnyObjectByType<RoundController>().currentRound;

        foreach (PlayerInput pi in PlayerManager.Instance.players)
        {
            if (pi == null) continue;

            Transform playerNoPI = pi.transform.Find("PlayerNoPI");
            if (playerNoPI == null) continue;

            // check if player is alive
            var health = playerNoPI.GetComponent<PlayerHealthSystem>();
            if (health == null || health.currentHealth < 0) continue;

            Vector3 position = playerNoPI.position;
            string data = $"{round+1},Position,{pi.gameObject.name},{position}";

            TestingLogger.LogToCSV(data);

            Debug.Log($"{sessionID}: player {pi.gameObject.name} at XY position ({position.x},{position.y})");
        }
    }
}
