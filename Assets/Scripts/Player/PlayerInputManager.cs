using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private PlayerShoot playerShoot;

    public void OnMove(InputValue value)
    {
        playerMovement.Move(value);
    }

    public void OnShoot()
    {
        playerShoot.ShootLogic();
    }
}
