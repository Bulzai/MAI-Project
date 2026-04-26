using TMPro;
using UnityEngine;

public class MilkCountdown : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private TMP_Text text;

    private float timer;
    private Vector3 originalLocalPos;

    public void Init(float time)
    {
        duration = time;
        timer = time;
    }

    private void Awake()
    {
        if (text != null)
            originalLocalPos = text.transform.localPosition;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0f) timer = 0f;

        if (text != null)
        {
            int display = Mathf.CeilToInt(timer);
            text.text = display.ToString();

            Color normalColor = new Color(224f / 255f, 187f / 255f, 120f / 255f); // #E0BB78

            // Reset to ORIGINAL position (not zero!)
            text.transform.localPosition = originalLocalPos;

            if (display <= 5)
            {
                text.color = Color.red;

                if (display <= 3)
                {
                    float targetScale = (display == 3) ? 1.35f : 1.25f;
                    text.transform.localScale = Vector3.one * targetScale;

                    float shakeAmount = 0.3f;

                    text.transform.localPosition = originalLocalPos + new Vector3(
                        Random.Range(-shakeAmount, shakeAmount),
                        Random.Range(-shakeAmount, shakeAmount),
                        0f
                    );
                }
            }
            else
            {
                text.color = normalColor;
                text.transform.localScale = Vector3.one;
            }
        }
    }
}