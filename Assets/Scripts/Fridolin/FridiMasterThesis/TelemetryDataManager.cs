using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

public class TelemetryDataManager : MonoBehaviour
{
    public void OnRoundEnded()
    {
        var scoreEvent = new RoundScore
        {
            roundIndex = 3,
            roundMilkCollected = 7,
            roundHealthLost = 25,
            roundDistanceTravelled = 12450,
            roundHitsTaken = 5,
            roundDamageDealt = 300,
            roundBlockedZoneCoveragePercent = 75,
            previousGamesPlayedBySamePlayer = 42,
            roundDurationSeconds = 180,
        };

        AnalyticsService.Instance.RecordEvent(scoreEvent);
        
    }
}
