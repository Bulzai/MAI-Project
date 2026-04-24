using System.Collections;
using TMPro;
using UnityEngine;

public class GlobalEventBannerUI : MonoBehaviour
{
    public static GlobalEventBannerUI Instance;

    [SerializeField] private GameObject bannerRoot;
    [SerializeField] private TMP_Text bannerText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform bannerRect;

    [Header("Animated Background")]
    [SerializeField] private Animator bannerAnimator;

    [Header("Animation")]
    [SerializeField] private float showDuration = 1.2f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float moveDistance = 40f;

    private Coroutine currentRoutine;
    private Vector2 originalPos;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (bannerRect != null)
            originalPos = bannerRect.anchoredPosition;

        HideImmediately();
    }

    public void ShowBanner(string message, Color textColor)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowBannerRoutine(message, textColor));
    }

    private IEnumerator ShowBannerRoutine(string message, Color textColor)
    {
        if (bannerRoot == null || bannerText == null || canvasGroup == null || bannerRect == null)
            yield break;

        bannerRoot.SetActive(true);

        if (bannerAnimator != null)
        {
            bannerAnimator.enabled = true;
            bannerAnimator.Play(0, 0, 0f);
        }

        bannerText.text = message;
        bannerText.color = textColor;

        canvasGroup.alpha = 0f;
        bannerRect.anchoredPosition = originalPos + new Vector2(0f, -moveDistance);

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);

            canvasGroup.alpha = p;
            bannerRect.anchoredPosition = Vector2.Lerp(
                originalPos + new Vector2(0f, -moveDistance),
                originalPos,
                p
            );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        bannerRect.anchoredPosition = originalPos;

        yield return new WaitForSeconds(showDuration);

        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / fadeDuration);

            canvasGroup.alpha = 1f - p;
            bannerRect.anchoredPosition = Vector2.Lerp(
                originalPos,
                originalPos + new Vector2(0f, moveDistance),
                p
            );

            yield return null;
        }

        HideImmediately();
        currentRoutine = null;
    }

    private void HideImmediately()
    {
        if (bannerAnimator != null)
        {
            bannerAnimator.enabled = false;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (bannerRect != null)
            bannerRect.anchoredPosition = originalPos;

        if (bannerRoot != null)
            bannerRoot.SetActive(false);
    }
}