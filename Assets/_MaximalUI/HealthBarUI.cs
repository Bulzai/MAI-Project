using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public int maxHealth = 100;

    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;

    private float pulseSpeed = 0f;
    private Color currentBaseColor = Color.green;

    void Update()
    {
        if (pulseSpeed > 0f)
        {
            float pulse = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed));
            float brightness = Mathf.Lerp(0.75f, 1f, pulse);

            fillImage.color = new Color(
                currentBaseColor.r * brightness,
                currentBaseColor.g * brightness,
                currentBaseColor.b * brightness,
                currentBaseColor.a
            );
        }
        else
        {
            fillImage.color = currentBaseColor;
        }
    }

    public void SetHealth(int currentHealth)
    {
        float value = Mathf.Clamp01((float)currentHealth / maxHealth);

        fillImage.fillAmount = value;

        // Base color from green -> red
        currentBaseColor = Color.Lerp(lowHealthColor, fullHealthColor, value);

        // Pulse speed depending on health
        if (value < 0.3f)
            pulseSpeed = 6f;
        else if (value < 0.6f)
            pulseSpeed = 3f;
        else
            pulseSpeed = 0f;
    }
}