using TMPro;
using UnityEngine;

public class MilkCountdown : MonoBehaviour
{
    [SerializeField] private float duration = 10f;
    [SerializeField] private TMP_Text text;

    private float timer;

    public void Init(float time)
    {
        duration = time;
        timer = time;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer < 0f) timer = 0f;

        if (text != null)
        {
            text.text = Mathf.CeilToInt(timer).ToString();
        }
    }
}