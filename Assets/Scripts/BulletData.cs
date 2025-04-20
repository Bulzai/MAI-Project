using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BulletData", menuName = "Scriptable Stats/BulletData")]
public class BulletData : ScriptableObject
{
    public float speed = 10f;
    public float maxDistance = 10f;
    public float damage = 1f;
    public float cooldown = 0.5f;
    public Sprite bulletSprite;
    public GameObject hitEffectPrefab;
    public GameObject bulletPrefab;

}

