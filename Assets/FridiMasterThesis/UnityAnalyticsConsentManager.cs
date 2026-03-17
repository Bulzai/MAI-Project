using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Analytics;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UnityAnalyticsConsentManager : MonoBehaviour
{
    public static UnityAnalyticsConsentManager Instance;
    
    private const int MAX_RETRIES = 5;
    private const int RETRY_DELAY_MS = 2000;
    private bool _servicesInitialized = false;
    
    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize services immediately (non-blocking) with retries
        _ = InitializeServicesAsync();
    }
    
    private async Task InitializeServicesAsync()
    {
        for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    var options = new InitializationOptions();

                    options.SetEnvironmentName("testing");
                    await UnityServices.InitializeAsync(options);
                }
                
                _servicesInitialized = true;
                Debug.Log($"Unity Services initialized successfully on attempt {attempt}");
                return; // Success - exit retry loop
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Services init attempt {attempt}/{MAX_RETRIES} failed: {e.Message}");
                
                if (attempt < MAX_RETRIES)
                {
                    await Task.Delay(RETRY_DELAY_MS);
                }
            }
        }
        
        // All retries failed
        await LogInitFailure();
        Debug.Log("Unity Services initialization FAILED after all retries");
    }
    
    private async Task LogInitFailure()
    {
        string logPath = Path.Combine(Application.persistentDataPath, "analytics_init_failed.txt");
        string errorLog = $"[{DateTime.Now}] Analytics services init failed after {MAX_RETRIES} retries.\n" +
                         $"Build: {Application.version} on {Application.platform}\n" +
                         "Check internet connection, Unity Dashboard project settings.";
        
        await File.WriteAllTextAsync(logPath, errorLog);
        Debug.Log($"Init failure logged to: {logPath}");
    }
    
    public async Task<bool> StartDataCollectionAsync()
    {
        // Already initialized (or failed) from startup
        if (!_servicesInitialized)
        {
            Debug.LogWarning("Services failed to initialize at startup");
            return false;
        }
        
        try
        {
            AnalyticsService.Instance.StartDataCollection();
            Debug.Log("Data collection ENABLED (consent granted)");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start data collection: {e.Message}");
            string logPath = Path.Combine(Application.persistentDataPath, "start_data_collection_failed.txt");
            string errorLog = $"[{DateTime.Now}] Analytics services init failed after {MAX_RETRIES} retries.\n" +
                              $"Build: {Application.version} on {Application.platform}\n" +
                              "Check internet connection, Unity Dashboard project settings.";
        
            await File.WriteAllTextAsync(logPath, errorLog);
            Debug.Log($"Init failure logged to: {logPath}");
            return false;
        }
    }
}
