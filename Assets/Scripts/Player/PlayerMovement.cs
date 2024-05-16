using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    private Vector2 movementInput;

    public float speed = 3.0f;


    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Move(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    public void MoveLogic()
    {
        _rigidbody2D.velocity = (movementInput * speed) * Time.deltaTime;
    }
}
