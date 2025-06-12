using UnityEngine;
using System.Collections;

public class BlinkFadeOut : MonoBehaviour
{
    public float lifetime = 10f;
    public float blinkDuration = 3f;

    private Material mat;
    private Color originalColor;

    private void Start()
    {
        // Get material (assumes one Renderer and one Material)
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;

        StartCoroutine(BlinkThenDestroy());
    }

    private IEnumerator BlinkThenDestroy()
    {
        yield return new WaitForSeconds(lifetime - blinkDuration);

        float time = 0f;

        while (time < blinkDuration)
        {
            float t = Mathf.PingPong(time * BlinkSpeed(time), 1f);
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(0.1f, 1f, t);
            mat.color = newColor;

            time += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private float BlinkSpeed(float time)
    {
        // Increase speed as time progresses
        return 2f + (time / blinkDuration) * 8f; // starts at 2, ends around 10
    }

}
