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


    public void LogRoundScore(int roundIndex, int durationSeconds, int milkCollected,
        int healthLost, int distanceTravelled, int hitsTaken, int damageDealt, int blockedCoverage,
        int previousGamesPlayed)
    {
        Debug.Log("sessionfilepath: " + sessionFilePath);
        Debug.Log($"Logs at: {Application.persistentDataPath}");

        if (sessionFilePath == null)
        {
            CreateSessionFile();
        }

        var scoreEvent = new LocalRoundScoreEvent
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
    
        LogRoundScoreEvent(scoreEvent);

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

    public Task LogTotalScore( /* your TotalScore parameters here - same pattern */){
        // Similar implementation to LogRoundScore, but with TotalScore parameters
        return Task.CompletedTask;
    }

    private void LogRoundScoreEvent(LocalRoundScoreEvent scoreEvent)
    {
        scoreEvent.timestamp = DateTime.UtcNow.ToString("o");
        scoreEvent.userId = userId;
        scoreEvent.sessionId = sessionId;

        string jsonLine = JsonUtility.ToJson(scoreEvent, true) + "\n";
        File.AppendAllText(sessionFilePath, jsonLine);
        Debug.Log($"Logged: {jsonLine}");
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
public class LocalRoundTotalScoreEvent
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
