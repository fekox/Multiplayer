using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerSprite;

    private Rigidbody2D _rigidbody2D;

    private Vector2 movementInput;

    private NetVector2 netVector2 = new NetVector2();

    [Header("Setup")]
    public float speed = 3.0f;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Move(InputValue value)
    {
        movementInput = value.Get<Vector2>();

        netVector2.data.Item2 = movementInput;

        NetworkManager.Instance.SendToServer(netVector2.Serialize());
    }
    
    public Vector2 GetPlayerPosition() 
    {
        return movementInput;
    }

    public void MoveLogic(Vector2 playerPos)
    {
        _rigidbody2D.velocity = (playerPos * speed) * Time.deltaTime;

        PlayerFlipX();
        PlayerFlipY();
    }

    public void PlayerFlipX() 
    {
        if (movementInput.x > 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(playerSprite.transform.rotation.x, playerSprite.transform.rotation.y, -90f);
        }

        if (movementInput.x < 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(playerSprite.transform.rotation.x, playerSprite.transform.rotation.y, 90f);
        }
    }

    public void PlayerFlipY() 
    {
        if (movementInput.y > 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(playerSprite.transform.rotation.x, playerSprite.transform.rotation.y, 0f);
        }

        if (movementInput.y < 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(playerSprite.transform.rotation.x, playerSprite.transform.rotation.y, 180f);       
        }
    }
}
