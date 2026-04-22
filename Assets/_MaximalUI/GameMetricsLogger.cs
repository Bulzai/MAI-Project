using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMetricsLogger : MonoBehaviour
{
    public static GameMetricsLogger Instance;

    public string uiVersion = "Maximal";
    public int sessionID = 1;

    private string sessionCode;
    private float roundStartTime;
    private int currentRound = 0;

    private Dictionary<int, float> survivalTimes = new Dictionary<int, float>();
    private Dictionary<int, int> deaths = new Dictionary<int, int>();
    private Dictionary<int, int> milkCollected = new Dictionary<int, int>();

    private string filePath;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        sessionCode = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        filePath = Path.Combine(Application.dataPath, "../metrics.csv");
        Debug.Log("Saving metrics to: " + filePath);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "UIVersion,SessionID,Round,PlayerIndex,SurvivalTime,Deaths,MilkCollected\n");
        }
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += StartRound;
        GameEvents.OnScoreStateEntered += EndRound;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= StartRound;
        GameEvents.OnScoreStateEntered -= EndRound;
    }

    private void StartRound()
    {
        currentRound++;
        roundStartTime = Time.time;

        survivalTimes.Clear();
        deaths.Clear();
        milkCollected.Clear();

        foreach (var player in PlayerManager.Instance.players)
        {
            if (player == null) continue;

            int playerIndex = player.playerIndex;
            survivalTimes[playerIndex] = 0f;
            deaths[playerIndex] = 0;
            milkCollected[playerIndex] = 0;
        }

        Debug.Log("Round started: " + currentRound);
    }

    private void EndRound()
    {
        float roundDuration = Time.time - roundStartTime;

        foreach (int playerIndex in new List<int>(survivalTimes.Keys))
        {
            if (deaths[playerIndex] == 0)
            {
                survivalTimes[playerIndex] = roundDuration;
            }

            string line = uiVersion + "," +
              SessionData.SessionID + "," +
              currentRound + "," +
              playerIndex + "," +
              survivalTimes[playerIndex].ToString("F2") + "," +
              deaths[playerIndex] + "," +
              milkCollected[playerIndex] + "\n";

            File.AppendAllText(filePath, line);
        }

        Debug.Log("Round data saved to: " + filePath);
    }

    public void RegisterDeath(int playerIndex)
    {
        if (deaths.ContainsKey(playerIndex))
            deaths[playerIndex] = 1;

        if (survivalTimes.ContainsKey(playerIndex))
            survivalTimes[playerIndex] = Time.time - roundStartTime;
    }

    public void RegisterMilkCollected(int playerIndex)
    {
        if (milkCollected.ContainsKey(playerIndex))
            milkCollected[playerIndex]++;
    }
}