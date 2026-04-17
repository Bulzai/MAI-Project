using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public int maxHealth = 100;

    public void SetHealth(int currentHealth)
    {
        float value = Mathf.Clamp01((float)currentHealth / maxHealth);
        fillImage.fillAmount = value;
    }
}