using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BulletData data;
    private Vector2 direction;
    private Vector2 startPosition;

    public void Initialize(Vector2 dir, BulletData bulletData)
    {
        data = bulletData;
        direction = dir.normalized;
        startPosition = transform.position;

        if (GetComponent<SpriteRenderer>() && data.bulletSprite)
            GetComponent<SpriteRenderer>().sprite = data.bulletSprite;
    }

    void Update()
    {
        transform.Translate(direction * data.speed * Time.deltaTime);

        if (Vector2.Distance(startPosition, transform.position) > data.maxDistance)
            Destroy(gameObject);
    }
}
