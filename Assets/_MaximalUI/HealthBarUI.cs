using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public int maxHealth = 100;

    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;

    [Header("Aura Indicator")]
    [SerializeField] private Image auraIconImage;
    [SerializeField] private Sprite pushAuraIcon;
    [SerializeField] private Sprite confuseAuraIcon;
    [SerializeField] private Sprite slowAuraIcon;
    [SerializeField] private Sprite damageAuraIcon;
    [SerializeField] private Sprite speedAuraIcon;

    private float pulseSpeed = 0f;
    private Color currentBaseColor = Color.green;

    private void Awake()
    {
        HideAuraIcon();
    }

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

        currentBaseColor = Color.Lerp(lowHealthColor, fullHealthColor, value);

        if (value < 0.3f)
            pulseSpeed = 6f;
        else if (value < 0.6f)
            pulseSpeed = 3f;
        else
            pulseSpeed = 0f;
    }

    public void ShowAuraIcon(PickUpItem.ItemType itemType)
    {
        if (auraIconImage == null) return;

        Sprite selectedSprite = null;

        switch (itemType)
        {
            case PickUpItem.ItemType.Repel:
                selectedSprite = pushAuraIcon;
                break;

            case PickUpItem.ItemType.Confusion:
                selectedSprite = confuseAuraIcon;
                break;

            case PickUpItem.ItemType.Slow:
                selectedSprite = slowAuraIcon;
                break;

            case PickUpItem.ItemType.Damage:
                selectedSprite = damageAuraIcon;
                break;

            case PickUpItem.ItemType.Speed:
                selectedSprite = speedAuraIcon;
                break;
        }

        if (selectedSprite == null) return;

        auraIconImage.sprite = selectedSprite;
        auraIconImage.gameObject.SetActive(true);
    }

    public void HideAuraIcon()
    {
        if (auraIconImage == null) return;

        auraIconImage.sprite = null;
        auraIconImage.gameObject.SetActive(false);
    }
}