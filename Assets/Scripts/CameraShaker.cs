using System.Collections;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    private Vector3 originalPosition;

    private void Awake()
    {
        originalPosition = transform.position;
    }

    public void ShakeCamera(float intensity, float duration)
    {
        StartCoroutine(Shake(intensity, duration));
    }

    private IEnumerator Shake(float intensity, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            Vector3 shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * intensity;
            transform.position = originalPosition + shakeOffset;
            yield return null;
        }

        transform.position = originalPosition;
    }
}
