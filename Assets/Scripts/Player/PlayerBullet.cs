using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [Header("SetUp")]
    [SerializeField] private float bulletSpeed = 50f;

    [SerializeField] private Rigidbody2D _rigidbody2D;

    [SerializeField] private float bulletLifeTime = 4f;

    private float maxBulletLifeTime;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }
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
}
