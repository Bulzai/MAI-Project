using UnityEngine;

public class Pulsator : MonoBehaviour
{
    [Header("Einstellungen")]
    [Tooltip("Wie schnell das Objekt pulsiert")]
    public float speed = 5f;

    [Tooltip("Der Multiplikator für die Größe (z.B. 1.2 für 20% Vergrößerung)")]
    public float pulseFactor = 1.2f;

    private Vector3 baseScale;

    void Start()
    {
        // Speichere die ursprüngliche Größe beim Start
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Erzeuge einen Wert, der zwischen 0 und 1 hin- und herpendelt
        float pulse = Mathf.PingPong(Time.time * speed, 1f);

        // Berechne die neue Skalierung: 
        // Wir interpolieren von baseScale bis (baseScale * pulseFactor)
        transform.localScale = Vector3.Lerp(baseScale, baseScale * pulseFactor, pulse);
    }
}