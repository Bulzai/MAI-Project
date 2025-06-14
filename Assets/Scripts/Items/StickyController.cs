using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyController : MonoBehaviour
{
    public float stickiness = 0.0f;
    public float dragWhileStuck = 100f;
    public float stickyDuration = 3f;

    // track stuck players and their timers
    private Dictionary<GameObject, Coroutine> stickyCoroutines = new Dictionary<GameObject, Coroutine>();

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            GameObject player = collision.collider.gameObject;
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // sticky movement effects
                Vector2 vel = playerRb.velocity;
                vel.x *= stickiness;
                playerRb.velocity = vel;

                if (playerRb.velocity.y > 0)
                    playerRb.velocity = new Vector2(playerRb.velocity.x, 0f);

                playerRb.drag = dragWhileStuck;

                // reset sticky time
                if (stickyCoroutines.ContainsKey(player))
                {
                    StopCoroutine(stickyCoroutines[player]);
                    stickyCoroutines[player] = StartCoroutine(StickyTimer(playerRb, player));
                }
                else
                {
                    stickyCoroutines[player] = StartCoroutine(StickyTimer(playerRb, player));
                }
            }
        }
    }

    private IEnumerator StickyTimer(Rigidbody2D playerRb, GameObject player)
    {
        yield return new WaitForSeconds(stickyDuration);

        // release player from sticky
        if (playerRb != null)
        {
            playerRb.drag = 0f;
            Debug.Log("Player released from sticky tile after timer.");
        }

        stickyCoroutines.Remove(player);
    }
}
