using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("SetUp")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    public void ShootLogic()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}
