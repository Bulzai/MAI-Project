using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BulletData data;
    private Vector2 direction;
    private Vector2 startPosition;

    void Start()
    {
        if (data == null)
            Debug.LogError("Bullet data not set!");
    }
    public void Initialize(Vector2 dir, BulletData bulletData)
    {
        data = bulletData;
        direction = dir.normalized;
        startPosition = transform.position;
        Debug.Log("Bullet initialized with speed: " + data.speed);

        if (GetComponent<SpriteRenderer>() && data.bulletSprite)
            GetComponent<SpriteRenderer>().sprite = data.bulletSprite;
    }

    void Update()
    {
        GetComponent<Rigidbody2D>().velocity = direction * data.speed;

        if (Vector2.Distance(startPosition, transform.position) > data.maxDistance)
            Destroy(gameObject);
    }
}
