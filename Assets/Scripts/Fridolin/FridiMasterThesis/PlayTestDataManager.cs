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
    private string smartPlacementQuestionnaireUrl = "https://forms.gle/sHwD4YwLMEZendz27";
    private string randomPlacementQuestionnaireUrl = "";
    public static PlayTestDataManager Instance { get; private set; }


    private int blockedZoneCoverageSum;
    private IntCounter nrTimesAddedToBlockedZoneCoverageSum { get; } = new();
    public int previousGamesPlayed { get; private set; } = 0;
    
    // Round totals (reset per round)
    public IntCounter roundIndex { get; } = new();
    public IntCounter roundDurationSeconds { get; } = new();
    public IntCounter roundMilkCollected { get; } = new();
    public IntCounter roundHealthLost { get; } = new();
    public IntCounter roundDistanceTravelled { get; } = new();
    public IntCounter roundHitsTaken { get; } = new();
    public IntCounter roundDamageDealt { get; } = new();
    public IntCounter roundBlockedZoneCoveragePercent { get; } = new();
    
    
    // Game totals (reset per game)
    public IntCounter totalDurationSeconds { get; } = new();
    public IntCounter totalMilkCollected { get; } = new();
    public IntCounter totalHealthLost { get; } = new();
    public IntCounter totalDistanceTravelled { get; } = new();
    public IntCounter totalHitsTaken { get; } = new();
    public IntCounter totalDamageDealt { get; } = new();
    public IntCounter totalBlockedZoneCoveragePercent { get; } = new();
    
    

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sessionId = Guid.NewGuid().ToString("N")[..8];
        userId = LoadOrCreateUserId();
        CreatePlaytestFolder();
        LoadPreviousGamesPlayed();
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
            File.WriteAllText(path, previousGamesPlayed.ToString());  // Just "17"
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
        if (!Directory.Exists(playtestFolder))
        {
            Directory.CreateDirectory(playtestFolder);
        }
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
        
        sessionFilePath = Path.Combine(playtestFolder, $"{sessionId}.jsonl");
        Debug.Log("sessionfilepath: " + sessionFilePath);
        Debug.Log($"Logs at: {Application.persistentDataPath}");


        LogGameInitializedEvent(new InitGameEvent
        {
            timestamp = DateTime.UtcNow.ToString("o"),
            userId = userId,
            sessionId = sessionId
        });
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
            totalDurationSeconds = totalDurationSeconds.Value,
            totalMilkCollected = totalMilkCollected.Value,
            totalHealthLost = totalHealthLost.Value,
            totalDistanceTravelled = totalDistanceTravelled.Value,
            totalHitsTaken = totalHitsTaken.Value,
            totalDamageDealt = totalDamageDealt.Value,
            totalBlockedZoneCoveragePercent = totalBlockedZoneCoveragePercent.Value
        };
    
        localTotalScoreEvent.timestamp = DateTime.UtcNow.ToString("o");
        localTotalScoreEvent.userId = userId;
        localTotalScoreEvent.sessionId = sessionId;

        string jsonLine = JsonUtility.ToJson(localTotalScoreEvent, true) + "\n";
        File.AppendAllText(sessionFilePath, jsonLine);
        Debug.Log($"Logged local TotalScore event");

    
        // Try online (Unity Analytics)
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            try
            {
                var analyticsTotalScoreEvent = new TotalScore
                {
                    totalDurationSeconds = totalDurationSeconds.Value,
                    totalMilkCollected = totalMilkCollected.Value,
                    totalHealthLost = totalHealthLost.Value,
                    totalDistanceTravelled = totalDistanceTravelled.Value,
                    totalHitsTaken = totalHitsTaken.Value,
                    totalDamageDealt = totalDamageDealt.Value,
                    totalBlockedZoneCoveragePercent = totalBlockedZoneCoveragePercent.Value
                };
                AnalyticsService.Instance.RecordEvent(analyticsTotalScoreEvent);
                Debug.Log("Logged online TotalScore event");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Online send failed: {e.Message}");
            }
        }
        ResetAllCounters();
        //TODO 
        // fix all excpetion handling and implement LogPlaythroughPlacementStrategy(); which logs the strategy to a txt or json file in playtestfolder
    }

    public void LogRoundScore()
    {
        Debug.Log("sessionfilepath: " + sessionFilePath);
        Debug.Log($"Logs at: {Application.persistentDataPath}");

        if (sessionFilePath == null)
        {
            CreateSessionFile();
        }

        var localRoundScoreEvent = new LocalRoundScoreEvent
        {
            roundIndex = roundIndex.Value,
            roundDurationSeconds = roundDurationSeconds.Value,
            roundMilkCollected = roundMilkCollected.Value,
            roundHealthLost = roundHealthLost.Value,
            roundDistanceTravelled = roundDistanceTravelled.Value,
            roundHitsTaken = roundHitsTaken.Value,
            roundDamageDealt = roundDamageDealt.Value,
            roundBlockedZoneCoveragePercent = roundBlockedZoneCoveragePercent.Value,
            previousGamesPlayedBySamePlayer = previousGamesPlayed
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
                    roundDurationSeconds = roundDurationSeconds.Value,
                    roundMilkCollected = roundMilkCollected.Value,
                    roundHealthLost = roundHealthLost.Value,
                    roundDistanceTravelled = roundDistanceTravelled.Value,
                    roundHitsTaken = roundHitsTaken.Value,
                    roundDamageDealt = roundDamageDealt.Value,
                    roundBlockedZoneCoveragePercent = roundBlockedZoneCoveragePercent.Value,
                    previousGamesPlayedBySamePlayer = previousGamesPlayed
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
        totalDurationSeconds.Increment(roundDurationSeconds.Value);
        totalMilkCollected.Increment(roundMilkCollected.Value);
        totalHealthLost.Increment(roundHealthLost.Value);
        totalDistanceTravelled.Increment(roundDistanceTravelled.Value);
        totalHitsTaken.Increment(roundHitsTaken.Value);
        totalDamageDealt.Increment(roundDamageDealt.Value);
        blockedZoneCoverageSum+= roundBlockedZoneCoveragePercent.Value;
        nrTimesAddedToBlockedZoneCoverageSum.Increment();
        totalBlockedZoneCoveragePercent.Reset();
        totalBlockedZoneCoveragePercent.Increment(blockedZoneCoverageSum/ nrTimesAddedToBlockedZoneCoverageSum.Value);
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
    

#if UNITY_EDITOR || UNITY_STANDALONE
#endif

    public void OpenPlaytestFolder()
    {
        string logFolder = playtestFolder;
    
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        // Windows Explorer
        Process.Start("explorer.exe", $"/open,\"{Application.persistentDataPath.Replace("/", "\\")}\"");
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        // Mac Finder  
        Process.Start("open", logFolder);
#elif UNITY_STANDALONE_LINUX
        // Linux File Manager
        Process.Start("xdg-open", logFolder);
#else
        // Mobile/WebGL - copy path to clipboard or show message
        Debug.Log($"Logs at: {logFolder}");
        Application.OpenURL(logFolder);  // May work on some platforms
#endif
    }
    
    public void ResetAllRoundCounters()
    {
        roundIndex.Reset();
        roundDurationSeconds.Reset();
        roundMilkCollected.Reset();
        roundHealthLost.Reset();
        roundDistanceTravelled.Reset();
        roundHitsTaken.Reset();
        roundDamageDealt.Reset();
        roundBlockedZoneCoveragePercent.Reset();
    }
    
    
    public void ResetAllTotalScoreCounters()
    {
        totalDurationSeconds.Reset();
        totalMilkCollected.Reset();
        totalHealthLost.Reset();
        totalDistanceTravelled.Reset();
        totalHitsTaken.Reset();
        totalDamageDealt.Reset();
        totalBlockedZoneCoveragePercent.Reset();
        blockedZoneCoverageSum = 0;
        nrTimesAddedToBlockedZoneCoverageSum.Reset();
    }

    public void ResetAllCounters()
    {
        ResetAllTotalScoreCounters();
        ResetAllRoundCounters();
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
    public int roundDurationSeconds;
    public int roundMilkCollected;
    public int roundHealthLost;
    public int roundDistanceTravelled;
    public int roundHitsTaken;
    public int roundDamageDealt;
    public int roundBlockedZoneCoveragePercent;
    public int previousGamesPlayedBySamePlayer;
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
    
    public int totalDurationSeconds;
    public int totalMilkCollected;
    public int totalHealthLost;
    public int totalDistanceTravelled;
    public int totalHitsTaken;
    public int totalDamageDealt;
    public int totalBlockedZoneCoveragePercent;
}

