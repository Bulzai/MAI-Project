using UnityEngine;

public class BouncyController : MonoBehaviour
{
    public float desiredHeight = 400f;
    public float bounceDuration = 0.3f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        Vector2 toPlayer = (rb.transform.position - transform.position).normalized;
        Vector2 padForward = transform.up; // pad's "front" direction

        // Check if the player is roughly in front of the pad (angle threshold)
        float dot = Vector2.Dot(toPlayer, padForward);
        if (dot > 0.5f) // Adjust this threshold for how strict "front" detection is
        {
            StartCoroutine(DoBounce(rb));
        }
    }

    private System.Collections.IEnumerator DoBounce(Rigidbody2D rb)
    {
        // reset velocity before bounce
        rb.velocity = Vector2.zero;

        // bounce direction from rotation
        Vector2 launchDirection = transform.up.normalized;
        float launchSpeed = Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * desiredHeight);

        rb.velocity = launchDirection * launchSpeed;

        // prevent movement script from overriding during bounce
        var movementScript = rb.GetComponent<MonoBehaviour>();
        if (movementScript != null)
        {
            movementScript.enabled = false;
        }

        yield return new WaitForSeconds(bounceDuration);

        if (movementScript != null)
        {
            movementScript.enabled = true;
        }
    }
}
