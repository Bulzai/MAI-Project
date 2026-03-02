using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Analytics;
using UnityEngine;
public class PlayTestDataManager : MonoBehaviour
{
    private string sessionFilePath;
    private string userId;
    private string sessionId;
    public static PlayTestDataManager Instance { get; private set; }


    private void Awake()
    { 
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        sessionId = Guid.NewGuid().ToString("N")[..8];
        userId = LoadOrCreateUserId();
    }

    private void Start()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            ChangeLocalDataToAnalyticsData();
        }
        else
        {
            UnityServices.Initialized += ChangeLocalDataToAnalyticsData;
        }
    }

    private void OnDestroy()
    {
        UnityServices.Initialized -= ChangeLocalDataToAnalyticsData;
    }

    private void ChangeLocalDataToAnalyticsData()
    {
        sessionId = AnalyticsService.Instance.SessionID;
        userId = AnalyticsService.Instance.GetAnalyticsUserID();
    }
    
    private static string LoadOrCreateUserId()
    {
        string userFile = Path.Combine(Application.persistentDataPath, "user_id.txt");
        if (File.Exists(userFile))
            return File.ReadAllText(userFile).Trim();

        string guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16]
            .Replace("/", "").Replace("+", "").Replace("=", "");
        File.WriteAllText(userFile, guid);
        return guid;
    }

    private void CreateSessionFile()
    {

        sessionFilePath = Path.Combine(Application.persistentDataPath, $"{sessionId}.jsonl");
        Debug.Log("sessionfilepath: " + sessionFilePath);
        Debug.Log($"Logs at: {Application.persistentDataPath}");


        LogEvent("SessionStart", new Dictionary<string, object>
        {
            ["userId"] = userId,
            ["sessionId"] = sessionId,
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["platform"] = Application.platform.ToString(),
            ["version"] = Application.version
        });
    }
    

    public void LogRoundScoreAsync(int roundIndex, int durationSeconds, int milkCollected,
        int healthLost, int distanceTravelled, int hitsTaken, int damageDealt, int blockedCoverage,
        int previousGamesPlayed)
    {
        Debug.Log("sessionfilepath: " + sessionFilePath);
        Debug.Log($"Logs at: {Application.persistentDataPath}");

        if (sessionFilePath == null)
        {
            CreateSessionFile();
        }
        // Always log locally (Python-ready JSONL)
        LogEvent("RoundScore", new Dictionary<string, object>
        {
            ["roundIndex"] = roundIndex,
            ["roundDurationSeconds"] = durationSeconds,
            ["roundMilkCollected"] = milkCollected,
            ["roundHealthLost"] = healthLost,
            ["roundDistanceTravelled"] = distanceTravelled,
            ["roundHitsTaken"] = hitsTaken,
            ["roundDamageDealt"] = damageDealt,
            ["roundBlockedZoneCoveragePercent"] = blockedCoverage,
            ["previousGamesPlayedBySamePlayer"] = previousGamesPlayed
        });

        // Try online (Unity Analytics)
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            try
            {
                var unityEvent = new RoundScore
                {
                    roundIndex = roundIndex,
                    roundDurationSeconds = durationSeconds,
                    roundMilkCollected = milkCollected,
                    roundHealthLost = healthLost,
                    roundDistanceTravelled = distanceTravelled,
                    roundHitsTaken = hitsTaken,
                    roundDamageDealt = damageDealt,
                    roundBlockedZoneCoveragePercent = blockedCoverage,
                    previousGamesPlayedBySamePlayer = previousGamesPlayed
                };
                AnalyticsService.Instance.RecordEvent(unityEvent);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Online send failed: {e.Message}");
            }
        }
    }

    public async Task LogTotalScoreAsync( /* your TotalScore parameters here - same pattern */)
    {
        // Same pattern: local first, online second
        LogEvent("TotalScore", new Dictionary<string, object>
        {
            // Add your total score fields
        });
        
    }

    private void LogEvent(string eventName, Dictionary<string, object> parameters)
    {
        var eventData = new Dictionary<string, object>
        {
            ["eventName"] = eventName,
            ["timestamp"] = DateTime.UtcNow.ToString("o"),
            ["userId"] = userId,
            ["sessionId"] = sessionId,
            ["parameters"] = parameters
        };
        string jsonLine = JsonUtility.ToJson(eventData, true) + "\n";
        File.AppendAllText(sessionFilePath, jsonLine);
        Debug.Log("log event done");
    }
}
