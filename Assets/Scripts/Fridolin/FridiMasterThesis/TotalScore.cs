using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalScore : Unity.Services.Analytics.Event
{
    public TotalScore() : base("totalScore")
    {
    }
    
    public int totalBlockedZoneCoveragePercent { set { SetParameter("totalBlockedZoneCoveragePercent", value); } }
    public int totalDamageDealt { set { SetParameter("totalDamageDealt", value); } }
    public int totalDistanceTravelled { set { SetParameter("totalDistanceTravelled", value); } }
    public int totalDurationSeconds { set { SetParameter("totalDurationSeconds", value); } }
    public int totalHealthLost { set { SetParameter("totalHealthLost", value); } }
    public int totalHitsTaken { set { SetParameter("totalHitsTaken", value); } }
    public int totalMilkCollected { set { SetParameter("totalMilkCollected", value); } }

}
