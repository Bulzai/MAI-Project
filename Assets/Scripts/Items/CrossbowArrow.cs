using UnityEngine;
using UnityEngine.InputSystem;   // for PlayerInput
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CrossbowArrow : MonoBehaviour
{
    [Tooltip("Seconds the arrow stays stuck before disappearing")]
    public float destroyDelay = 2f;

    Rigidbody2D _rb;
    FixedJoint2D _joint;
    PlayerInput _playerInput;
    bool _attached = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // 1) Hit the Player (first time only) → attach
        if (!_attached && col.collider.CompareTag("Player"))
        {
            AttachPlayer(col.collider.attachedRigidbody);
        }
        // 2) Hit anything that is NOT the Player → stop & schedule destroy
        else if (!col.collider.CompareTag("Player"))
        {
            StopAndScheduleDestroy();
        }
    }

    void AttachPlayer(Rigidbody2D playerRb)
    {
        if (playerRb == null) return;

        // add a joint so physics carries the player
        _joint = gameObject.AddComponent<FixedJoint2D>();
        _joint.connectedBody = playerRb;
        _attached = true;

        // disable the PlayerInput on the root so they can't move
        _playerInput = playerRb.transform.root.GetComponent<PlayerInput>();
        if (_playerInput != null)
            _playerInput.enabled = false;
    }

    void StopAndScheduleDestroy()
    {
        // 1) Detach player & restore controls if we were carrying
        if (_attached)
        {
            if (_joint != null) Destroy(_joint);
            if (_playerInput != null)
                _playerInput.enabled = true;
            _attached = false;
        }

        // 2) Stop arrow motion and disable its collider
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;

        // 3) Destroy after a delay
        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
