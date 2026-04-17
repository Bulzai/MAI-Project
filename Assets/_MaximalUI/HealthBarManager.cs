using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance;

    public HealthBarUI[] healthBars;

    private void Awake()
    {
        Instance = this;
    }

    public HealthBarUI GetHealthBar(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= healthBars.Length)
            return null;

        return healthBars[playerIndex];
    }
}