using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<GameObject> hearts;

    [SerializeField] private GameObject shield;

    [Header("SetUp")]
    [SerializeField] private int maxHealth;

    [SerializeField] private float inmortalTimer;

    [SerializeField] private string bulletTag = "Bullet";

    [SerializeField] private bool inmortal = false;

    private int curretHealth = 0;

    private bool isDead = false;

    private float timer;

    private void Start()
    {
        StartTank();

        timer = inmortalTimer;
    }

    private void Update()
    {
        if (inmortal)
        {
            if(timer > 0)
            {
                timer -= Time.deltaTime;
                shield.SetActive(true);
            }

            if(timer <= 0)
            {
                timer = inmortalTimer;
                inmortal = false;
                shield.SetActive(false);
            }
        }
    }

    public void StartTank() 
    {
        SetCurrentHealth(maxHealth);

        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].SetActive(true);
        }
    }

    public void TakeDamage(int damage)
    {
        if (curretHealth > 0)
        {
            curretHealth -= damage;

            for (int i = 0; i < damage; i++)
            {
                hearts[i].SetActive(false);
                hearts.Remove(hearts[i]);
            }
        }
    }

    public void UpdatePlayerHealth()
    {
        if (curretHealth <= 0)
        {
            isDead = true;
            SetCurrentHealth(0);
        }

        if(isDead)
        {
            gameObject.SetActive(false);
        }
    }

    public int GetMaxHealth() 
    {
        return maxHealth;
    }

    public void SetCurrentHealth(int newHealth) 
    {
        curretHealth = newHealth;
    }

    public int GetCurrentHealth() 
    {
        return curretHealth;
    }

    private void OnTriggerEnter2D(Collider2D bullet)
    {
        if (!isDead && !inmortal) 
        {
            if (bullet.gameObject.CompareTag(bulletTag))
            {
                inmortal = true;
                TakeDamage(1);
            }
        }
    }
}