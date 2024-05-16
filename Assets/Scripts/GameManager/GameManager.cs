using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private PlayerHealth playerHealth;

    void FixedUpdate()
    {
        playerMovement.MoveLogic();

        playerHealth.UpdatePlayerHealth();
    }
}
