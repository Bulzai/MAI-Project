using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance;
    public HealthBarUI[] healthBars;
    public GameObject healthBarsRoot;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += ShowBars;
        GameEvents.OnMainGameStateExited += HideBars;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= ShowBars;
        GameEvents.OnMainGameStateExited -= HideBars;
    }

    public HealthBarUI GetHealthBar(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= healthBars.Length)
            return null;

        return healthBars[playerIndex];
    }

    private void ShowBars()
    {
        if (healthBarsRoot != null)
            healthBarsRoot.SetActive(true);
    }

    private void HideBars()
    {
        if (healthBarsRoot != null)
            healthBarsRoot.SetActive(false);
    }
}