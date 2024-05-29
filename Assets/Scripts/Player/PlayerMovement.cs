using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerSprite;

    [SerializeField] private GameManager gameManager;

    [SerializeField] private Rigidbody2D _rigidbody2D;

    private Vector2 movementInput;

    private NetVector2 netVector2 = new NetVector2();

    [Header("Setup")]
    public float speed = 3.0f;

    public void Move(InputValue value)
    {
        movementInput = value.Get<Vector2>();

        netVector2.data.Item2 = movementInput;

        if (!NetworkManager.Instance.isServer) 
        {
            NetworkManager.Instance.SendToServer(netVector2.Serialize());
        }
    }
    
    public Vector2 GetPlayerPosition() 
    {
        return movementInput;
    }

    public void UpdatePlayerMovement(int id, Vector2 playerPos)
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();

        _rigidbody2D.velocity = (playerPos * speed);

        PlayerFlipX(playerPos);
        PlayerFlipY(playerPos);

        NetworkManager.Instance.playerList[id].position = playerPos;
    }

    public void PlayerFlipX(Vector2 playerPos) 
    {
        if (playerPos.x > 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(playerSprite.transform.rotation.x, playerSprite.transform.rotation.y, -90f);
        }

        if (playerPos.x < 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(playerSprite.transform.rotation.x, playerSprite.transform.rotation.y, 90f);
        }
    }

    public void PlayerFlipY(Vector2 playerPos) 
    {
        if (playerPos.y > 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(playerSprite.transform.rotation.x, playerSprite.transform.rotation.y, 0f);
        }

        if (playerPos.y < 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(playerSprite.transform.rotation.x, playerSprite.transform.rotation.y, 180f);       
        }
    }
}
