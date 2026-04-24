using UnityEngine;

public class GlobalGameplayBannerTrigger : MonoBehaviour
{
    [SerializeField] private int lowHealthThreshold = 30;

    private bool hurryUpShownThisRound = false;

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += ResetRoundFlags;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= ResetRoundFlags;
    }

    private void Update()
    {
        if (PlayerManager.Instance == null || GlobalEventBannerUI.Instance == null)
            return;

        CheckHurryUp();
    }

    private void ResetRoundFlags()
    {
        hurryUpShownThisRound = false;
    }

    private void CheckHurryUp()
    {
        if (hurryUpShownThisRound) return;

        int lowCount = 0;
        int aliveCount = 0;

        foreach (var player in PlayerManager.Instance.players)
        {
            if (player == null) continue;

            var playerObj = player.transform.Find("PlayerNoPI");
            if (playerObj == null || !playerObj.gameObject.activeSelf)
                continue;

            aliveCount++;

            var health = player.GetComponentInChildren<PlayerHealthSystem>();
            if (health != null && health.currentHealth <= lowHealthThreshold)
            {
                lowCount++;
            }
        }

        if (aliveCount == 2 && lowCount == 2)
        {
            hurryUpShownThisRound = true;
            GlobalEventBannerUI.Instance.ShowBanner("HURRY UP!", Color.red);
        }
    }
}