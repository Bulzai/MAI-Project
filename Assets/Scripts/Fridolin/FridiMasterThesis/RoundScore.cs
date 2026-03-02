using System;
using Unity.Services.Analytics;

public class RoundScore : Unity.Services.Analytics.Event
{
    public RoundScore() : base("RoundScore")
    {
    }

    public int roundIndex { set { SetParameter("roundIndex", value); } }
    public int roundDurationSeconds { set { SetParameter("roundDurationSeconds", value); } }
    public int roundMilkCollected { set { SetParameter("roundMilkCollected", value); } }
    public int roundHealthLost { set { SetParameter("roundHealthLost", value); } }
    public int roundDistanceTravelled { set { SetParameter("roundDistanceTravelled", value); } }
    public int roundHitsTaken { set { SetParameter("roundHitsTaken", value); } }
    public int roundDamageDealt { set { SetParameter("roundDamageDealt", value); } }
    public int roundBlockedZoneCoveragePercent { set { SetParameter("roundBlockedZoneCoveragePercent", value); } }
    public int previousGamesPlayedBySamePlayer { set { SetParameter("previousGamesPlayedBySamePlayer", value); } }
}