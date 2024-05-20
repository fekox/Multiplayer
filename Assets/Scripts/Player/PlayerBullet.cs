using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D _rigidbody2D;

    [Header("Setup")]
    [SerializeField] private float bulletSpeed = 50f;

    [SerializeField] private float bulletLifeTime = 4f;

    [SerializeField] private int bulletDamage;

    [SerializeField] private string playerTag = "Player";

    private float maxBulletLifeTime;

    private void Start()
    {
        maxBulletLifeTime = bulletLifeTime;
    }

    private void Update()
    {
        _rigidbody2D.velocity = transform.up * bulletSpeed;

        DestroyBullet();
    }

    public void DestroyBullet()
    {
        maxBulletLifeTime -= Time.deltaTime;

        if (maxBulletLifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D bullet)
    {
        if (bullet.gameObject.CompareTag(playerTag)) 
        {
            Destroy(gameObject);
        }
    }
}
