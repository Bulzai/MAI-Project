using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Analytics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PlayTestDataManager : MonoBehaviour
{
    private string sessionFilePath;
    private string userId;
    private string sessionId;
    private string playtestFolder;
    private string smartPlacementQuestionnaireUrl = "https://forms.gle/nXQBodPrwcAYAh5r6";
    private string randomPlacementQuestionnaireUrl = "https://forms.gle/GvTnHyinKXV4z7NX8";
    
    
    public static PlayTestDataManager Instance { get; private set; }


    public int previousGamesPlayed { get; set; } = 0;
    
    // Round totals (reset per round)
    public IntCounter roundIndex { get; } = new();
    public float roundDurationInSeconds;
    public IntCounter roundMilkCollected { get; } = new();
    public IntCounter roundHealthLost { get; } = new();
    public IntCounter roundDistanceTravelled { get; } = new();
    public IntCounter roundHitsTaken { get; } = new();
    public IntCounter roundDamageDealt { get; } = new();
    //is actually  just GetCoveredByAttackRangeCellCount
    public IntCounter roundBlockedZoneCoveragePercent { get; } = new();
    
    
    // Game totals (reset per game)
    public float totalDurationInSeconds;
    public IntCounter totalMilkCollected { get; } = new();
    public IntCounter totalHealthLost { get; } = new();
    public IntCounter totalDistanceTravelled { get; } = new();
    public IntCounter totalHitsTaken { get; } = new();
    public IntCounter totalDamageDealt { get; } = new();
    public IntCounter totalBlockedZoneCoveragePercent { get; } = new();
    
    
    //round timing stuff
    private bool isRunning = false;

    private void Awake()
    {
        Debug.Log("playtestdatamanager awake, initializing singleton and loading user/session data");
        if (Instance != null && Instance != this)
        {
            Debug.Log("Another instance of PlayTestDataManager already exists, destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sessionId = Guid.NewGuid().ToString("N")[..8];
        userId = LoadOrCreateUserId();
        CreatePlaytestFolder();
        LoadPreviousGamesPlayed();
        GameEvents.OnMainGameStateExited += StopRoundTimer;
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
        GameEvents.OnMainGameStateExited -= StopRoundTimer;
    }


    private void Update()
    {
        if ((isRunning))
        {
            roundDurationInSeconds += Time.deltaTime;
        }
    }
    
    
    private void LoadPreviousGamesPlayed()
    {
        string path = Path.Combine(playtestFolder, "previous_games_played.txt");
        try 
        {
            if (File.Exists(path)) 
            {
                string content = File.ReadAllText(path).Trim();
                if (int.TryParse(content, out int count))
                {
                    previousGamesPlayed = count;
                    return;  // Success!
                }
            }
        }
        catch (Exception e) 
        {
            Debug.LogError($"Load games played failed: {e.Message}");
        }
        Debug.Log($"Loaded: {previousGamesPlayed} prior games");
    }

    public void SavePreviousGamesPlayed()
    {
        string path = Path.Combine(playtestFolder, "previous_games_played.txt");
        try 
        {
            File.WriteAllText(path, previousGamesPlayed.ToString()); 
            Debug.Log($"Saved: {previousGamesPlayed} games");
        }
        catch (Exception e) 
        {
            Debug.LogError($"Save games played failed: {e.Message}");
            // Continue - don't crash playtest!
        }
    }
    
    private void CreatePlaytestFolder()
    {
        playtestFolder = Path.Combine(Application.persistentDataPath, "PlaytestData");
        try
        {
            if (!Directory.Exists(playtestFolder))
            {
                Directory.CreateDirectory(playtestFolder);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create playtest folder: {e.Message}");
            playtestFolder = Application.persistentDataPath; // Fallback
        }
    }
    private void ChangeLocalDataToAnalyticsData()
    {
        try
        {
            sessionId = AnalyticsService.Instance.SessionID;
            userId = AnalyticsService.Instance.GetAnalyticsUserID();
            CircleLoadAnim.playAnim = false;
            Debug.Log($"Analytics initialized. SessionID: {sessionId}, UserID: {userId}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Analytics service unavailable, using local IDs: {e.Message}");
        }
    }

    private static string LoadOrCreateUserId()
    {
        string userFile = Path.Combine(Application.persistentDataPath, "user_id.txt");
        try
        {
            if (File.Exists(userFile))
            {
                return File.ReadAllText(userFile).Trim();  // ← No try-catch!
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read user ID: {e.Message}");
        }

        string guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray())[..16]
            .Replace("/", "").Replace("+", "").Replace("=", "");
        try
        {
            File.WriteAllText(userFile, guid);  // ← No try-catch!
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write user ID: {e.Message}");
            // Still return GUID - don't crash
        }
        return guid;
    }

    private void CreateSessionFile()
    {
        
        sessionFilePath = Path.Combine(playtestFolder, $"{sessionId}.jsonl");
        Debug.Log("sessionfilepath: " + sessionFilePath);
        Debug.Log($"Logs at: {Application.persistentDataPath}");

        try
        {
            LogGameInitializedEvent(new InitGameEvent
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                userId = userId,
                sessionId = sessionId
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create session file: {e.Message}");
            sessionFilePath = null; // Mark as failed
        }
    }

    public void LogTotalScore()
    {
        Debug.Log("sessionfilepath: " + sessionFilePath);
        Debug.Log($"Logs at: {Application.persistentDataPath}");

        if (sessionFilePath == null)
        {
            CreateSessionFile();
        }

        var localTotalScoreEvent = new LocalTotalScoreEvent
        {
            totalDurationInSeconds = totalDurationInSeconds,
            totalMilkCollected = totalMilkCollected.Value,
            totalHealthLost = totalHealthLost.Value,
            totalDistanceTravelled = totalDistanceTravelled.Value,
            totalHitsTaken = totalHitsTaken.Value,
            totalDamageDealt = totalDamageDealt.Value,
            totalBlockedZoneCoveragePercent = totalBlockedZoneCoveragePercent.Value,
            totalPreviousGamesPlayedBySamePlayer = previousGamesPlayed,
            totalPlacementStrategy = AISelector.Instance.currentPlaythroughType.ToString()
        };
    
        localTotalScoreEvent.timestamp = DateTime.UtcNow.ToString("o");
        localTotalScoreEvent.userId = userId;
        localTotalScoreEvent.sessionId = sessionId;

        try
        {
            string jsonLine = JsonUtility.ToJson(localTotalScoreEvent, true) + "\n";
            File.AppendAllText(sessionFilePath, jsonLine);
            Debug.Log($"Logged local {localTotalScoreEvent.eventName} event");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to log {localTotalScoreEvent.eventName}: {e.Message}");
        }
        // Try online (Unity Analytics)
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            try
            {
                var analyticsTotalScoreEvent = new TotalScore
                {
                    totalDurationInSeconds = totalDurationInSeconds,
                    totalMilkCollected = totalMilkCollected.Value,
                    totalHealthLost = totalHealthLost.Value,
                    totalDistanceTravelled = totalDistanceTravelled.Value,
                    totalHitsTaken = totalHitsTaken.Value,
                    totalDamageDealt = totalDamageDealt.Value,
                    totalBlockedZoneCoveragePercent = totalBlockedZoneCoveragePercent.Value,
                    totalPlacementStrategy = AISelector.Instance.currentPlaythroughType.ToString(),
                    totalPreviousGamesPlayedBySamePlayer = previousGamesPlayed
                };
                AnalyticsService.Instance.RecordEvent(analyticsTotalScoreEvent);
                Debug.Log("Logged online TotalScore event");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Online send failed: {e.Message}");
            }
        }
  }
    
    

    public void LogRoundScore()
    {
        roundBlockedZoneCoveragePercent.Reset();
        roundBlockedZoneCoveragePercent.Increment(HeatMap.Instance.GetCoveredByAttackRangeCellCount());
        Debug.Log("sessionfilepath: " + sessionFilePath);
        Debug.Log($"Logs at: {Application.persistentDataPath}");

        if (sessionFilePath == null)
        {
            CreateSessionFile();
        }

        var localRoundScoreEvent = new LocalRoundScoreEvent
        {
            roundIndex = roundIndex.Value,
            roundDurationInSeconds = roundDurationInSeconds,
            roundMilkCollected = roundMilkCollected.Value,
            roundHealthLost = roundHealthLost.Value,
            roundDistanceTravelled = roundDistanceTravelled.Value,
            roundHitsTaken = roundHitsTaken.Value,
            roundDamageDealt = roundDamageDealt.Value,
            roundBlockedZoneCoveragePercent = roundBlockedZoneCoveragePercent.Value,
            roundPreviousGamesPlayedBySamePlayer = previousGamesPlayed,
            roundPlacementStrategy = AISelector.Instance.currentPlaythroughType.ToString()
        };
        
        localRoundScoreEvent.timestamp = DateTime.UtcNow.ToString("o");
        localRoundScoreEvent.userId = userId;
        localRoundScoreEvent.sessionId = sessionId;

        string jsonLine = JsonUtility.ToJson(localRoundScoreEvent, true) + "\n";
        File.AppendAllText(sessionFilePath, jsonLine);
        Debug.Log($"Logged local RoundScore event");

        
        // Try online (Unity Analytics)
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            try
            {
                var analyticsRoundScoreEvent = new RoundScore
                {
                    roundIndex = roundIndex.Value,
                    roundDurationInSeconds = roundDurationInSeconds,
                    roundMilkCollected = roundMilkCollected.Value,
                    roundHealthLost = roundHealthLost.Value,
                    roundDistanceTravelled = roundDistanceTravelled.Value,
                    roundHitsTaken = roundHitsTaken.Value,
                    roundDamageZoneCoveragePercent = roundBlockedZoneCoveragePercent.Value,
                    roundPlacementStrategy = AISelector.Instance.currentPlaythroughType.ToString(),
                    roundPreviousGamesPlayedBySamePlayer = previousGamesPlayed
                };
                AnalyticsService.Instance.RecordEvent(analyticsRoundScoreEvent);
                Debug.Log("Logged online RoundScore event");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Online send failed: {e.Message}");
            }
        }
        UpdateTotalScoresWithCurrentRound();
        ResetAllRoundCounters();
    }
    
    private void UpdateTotalScoresWithCurrentRound()
    {
        totalDurationInSeconds += roundDurationInSeconds;
        totalMilkCollected.Increment(roundMilkCollected.Value);
        totalHealthLost.Increment(roundHealthLost.Value);
        totalDistanceTravelled.Increment(roundDistanceTravelled.Value);
        totalHitsTaken.Increment(roundHitsTaken.Value);
        totalDamageDealt.Increment(roundDamageDealt.Value);
        totalBlockedZoneCoveragePercent.Reset();
        totalBlockedZoneCoveragePercent.Increment(roundBlockedZoneCoveragePercent.Value);
    }

    
    private void LogGameInitializedEvent(InitGameEvent initEvent)
    {
        string jsonLine = JsonUtility.ToJson(initEvent, true) + "\n";
        File.AppendAllText(sessionFilePath, jsonLine);
        Debug.Log($"Logged: {jsonLine}");
    }

    public void OpenSmartPlacementQuestionnaireLink()
    {
        Application.OpenURL(smartPlacementQuestionnaireUrl);
    }
    
    public void OpenRandomPlacementQuestionnaireLink()
    {
        Application.OpenURL(randomPlacementQuestionnaireUrl);
    }
    
    public void OpenPlaytestFolder()
    {
        string logFolder = playtestFolder;
        try
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Process.Start("explorer.exe", $"/open,\"{Application.persistentDataPath.Replace("/", "\\")}\"");
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            Process.Start("open", $"\"{logFolder}\"");
#elif UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", logFolder);
#else
        Debug.Log($"Logs at: {logFolder}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to open folder ({e.Message}). Path: {logFolder}");
        }
    }
    
    
    public void ResetAllRoundCounters()
    {
        roundDurationInSeconds = 0f;
        roundMilkCollected.Reset();
        roundHealthLost.Reset();
        roundDistanceTravelled.Reset();
        roundHitsTaken.Reset();
        roundDamageDealt.Reset();
        roundBlockedZoneCoveragePercent.Reset();
    }
    
    
    public void ResetAllTotalScoreCounters()
    {
        roundIndex.Reset();
        totalDurationInSeconds = 0f;
        totalMilkCollected.Reset();
        totalHealthLost.Reset();
        totalDistanceTravelled.Reset();
        totalHitsTaken.Reset();
        totalDamageDealt.Reset();
        totalBlockedZoneCoveragePercent.Reset();

    }

    public void ResetAllCounters()
    {
        ResetAllTotalScoreCounters();
        ResetAllRoundCounters();
    }
    
    
    public void StartRoundTimer()
    {
        isRunning = true;
    }
    
    public void StopRoundTimer()
    {
        isRunning = false;
    }

    public string GetSessionID()
    {
        return sessionId;
    }
    public string GetUserID()
    {
        return userId;
    }
    
}

[Serializable]
public class LocalRoundScoreEvent
{
    public string eventName = "RoundScore";
    public string timestamp;
    public string userId;
    public string sessionId;
    
    public int roundIndex;
    public float roundDurationInSeconds;
    public int roundMilkCollected;
    public int roundHealthLost;
    public int roundDistanceTravelled;
    public int roundHitsTaken;
    public int roundDamageDealt;
    public int roundBlockedZoneCoveragePercent;
    public int roundPreviousGamesPlayedBySamePlayer;
    public string roundPlacementStrategy;
}

[Serializable]
public class InitGameEvent
{
    public string eventName = "GameInitialized";
    public string timestamp;
    public string userId;
    public string sessionId;
    
}


[Serializable]
public class LocalTotalScoreEvent
{
    public string eventName = "totalScore";
    public string timestamp;
    public string userId;
    public string sessionId;
    
    public float totalDurationInSeconds;
    public int totalMilkCollected;
    public int totalHealthLost;
    public int totalDistanceTravelled;
    public int totalHitsTaken;
    public int totalDamageDealt;
    public int totalBlockedZoneCoveragePercent;
    public int totalPreviousGamesPlayedBySamePlayer;
    public string totalPlacementStrategy;
}

