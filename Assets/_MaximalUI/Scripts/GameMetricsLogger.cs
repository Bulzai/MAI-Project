using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameMetricsLogger : MonoBehaviour
{
    public static GameMetricsLogger Instance;

    private float roundStartTime;
    private int currentRound = 0;

    private Dictionary<int, float> survivalTimes = new();
    private Dictionary<int, int> deaths = new();
    private Dictionary<int, int> milkCollected = new();

    private Dictionary<int, int> totalAurasCollected = new();
    private Dictionary<int, int> slowAurasCollected = new();
    private Dictionary<int, int> repelAurasCollected = new();
    private Dictionary<int, int> confusionAurasCollected = new();
    private Dictionary<int, int> speedAurasCollected = new();

    private Dictionary<int, float> lastMilkCollectTime = new();
    private Dictionary<int, float> totalTimeBetweenMilk = new();
    private Dictionary<int, int> milkIntervalCount = new();

    // NEW
    private Dictionary<int, int> damageTaken = new();
    private Dictionary<int, int> jumpCount = new();
    private Dictionary<int, float> firstMilkTime = new();
    private Dictionary<int, float> firstAuraTime = new();

    private string filePath;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        filePath = Path.Combine(Application.dataPath, "../PLEASE_SEND_THIS_FILE_TO_ME_metrics.csv");
        Debug.Log("Saving metrics to: " + filePath);

        if (!File.Exists(filePath))
        {
            File.WriteAllText(
                filePath,
                "UIVersion,SessionID,Round,PlayerIndex,CharacterName,SurvivalTime,Deaths,MilkCollected," +
                "TotalAurasCollected,SlowAurasCollected,RepelAurasCollected,ConfusionAurasCollected,SpeedAurasCollected," +
                "AverageTimeBetweenMilk,DamageTaken,JumpCount,FirstMilkTime,FirstAuraTime\n"
            );
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

        totalAurasCollected.Clear();
        slowAurasCollected.Clear();
        repelAurasCollected.Clear();
        confusionAurasCollected.Clear();
        speedAurasCollected.Clear();

        lastMilkCollectTime.Clear();
        totalTimeBetweenMilk.Clear();
        milkIntervalCount.Clear();

        damageTaken.Clear();
        jumpCount.Clear();
        firstMilkTime.Clear();
        firstAuraTime.Clear();

        foreach (var player in PlayerManager.Instance.players)
        {
            if (player == null) continue;

            int playerIndex = player.playerIndex;

            survivalTimes[playerIndex] = 0f;
            deaths[playerIndex] = 0;
            milkCollected[playerIndex] = 0;

            totalAurasCollected[playerIndex] = 0;
            slowAurasCollected[playerIndex] = 0;
            repelAurasCollected[playerIndex] = 0;
            confusionAurasCollected[playerIndex] = 0;
            speedAurasCollected[playerIndex] = 0;

            lastMilkCollectTime[playerIndex] = -1f;
            totalTimeBetweenMilk[playerIndex] = 0f;
            milkIntervalCount[playerIndex] = 0;

            damageTaken[playerIndex] = 0;
            jumpCount[playerIndex] = 0;
            firstMilkTime[playerIndex] = -1f;
            firstAuraTime[playerIndex] = -1f;
        }

        Debug.Log("Round started: " + currentRound);
    }

    private void EndRound()
    {
        float roundDuration = Time.time - roundStartTime;

        foreach (int playerIndex in new List<int>(survivalTimes.Keys))
        {
            if (deaths[playerIndex] == 0)
                survivalTimes[playerIndex] = roundDuration;

            float averageMilkTime = 0f;
            if (milkIntervalCount.ContainsKey(playerIndex) && milkIntervalCount[playerIndex] > 0)
                averageMilkTime = totalTimeBetweenMilk[playerIndex] / milkIntervalCount[playerIndex];

            string line = SessionData.BuildType + "," +
                          SessionData.SessionID + "," +
                          currentRound + "," +
                          playerIndex + "," +
                          GetCharacterName(playerIndex) + "," +
                          survivalTimes[playerIndex].ToString("F2") + "," +
                          deaths[playerIndex] + "," +
                          milkCollected[playerIndex] + "," +
                          totalAurasCollected[playerIndex] + "," +
                          slowAurasCollected[playerIndex] + "," +
                          repelAurasCollected[playerIndex] + "," +
                          confusionAurasCollected[playerIndex] + "," +
                          speedAurasCollected[playerIndex] + "," +
                          averageMilkTime.ToString("F2") + "," +
                          damageTaken[playerIndex] + "," +
                          jumpCount[playerIndex] + "," +
                          firstMilkTime[playerIndex].ToString("F2") + "," +
                          firstAuraTime[playerIndex].ToString("F2") + "\n";

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
        if (!milkCollected.ContainsKey(playerIndex)) return;

        milkCollected[playerIndex]++;

        float currentTime = Time.time - roundStartTime;

        if (firstMilkTime[playerIndex] < 0f)
            firstMilkTime[playerIndex] = currentTime;

        if (lastMilkCollectTime[playerIndex] >= 0f)
        {
            float timeSinceLastMilk = currentTime - lastMilkCollectTime[playerIndex];
            totalTimeBetweenMilk[playerIndex] += timeSinceLastMilk;
            milkIntervalCount[playerIndex]++;
        }

        lastMilkCollectTime[playerIndex] = currentTime;
    }

    public void RegisterAuraCollected(int playerIndex, PickUpItem.ItemType itemType)
    {
        if (!totalAurasCollected.ContainsKey(playerIndex)) return;

        totalAurasCollected[playerIndex]++;

        if (firstAuraTime[playerIndex] < 0f)
            firstAuraTime[playerIndex] = Time.time - roundStartTime;

        switch (itemType)
        {
            case PickUpItem.ItemType.Slow:
                slowAurasCollected[playerIndex]++;
                break;

            case PickUpItem.ItemType.Repel:
                repelAurasCollected[playerIndex]++;
                break;

            case PickUpItem.ItemType.Confusion:
                confusionAurasCollected[playerIndex]++;
                break;

            case PickUpItem.ItemType.Speed:
                speedAurasCollected[playerIndex]++;
                break;
        }
    }

    public void RegisterDamageTaken(int playerIndex, int amount)
    {
        if (!damageTaken.ContainsKey(playerIndex)) return;
        damageTaken[playerIndex] += amount;
    }

    public void RegisterJump(int playerIndex)
    {
        if (!jumpCount.ContainsKey(playerIndex)) return;
        jumpCount[playerIndex]++;
    }

    private string GetCharacterName(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0:
                return "Cutesy";
            case 1:
                return "Jokesy";
            default:
                return "Unknown";
        }
    }
}