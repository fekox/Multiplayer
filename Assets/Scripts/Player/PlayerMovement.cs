using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerSprite;

    [SerializeField] private GameManager gameManager;

    private Vector3 movementInput;

    private NetVector3 netVector3 = new NetVector3();

    public int ID;

    [Header("Setup")]
    public float speed = 3.0f;

    private void Update()
    {
        if (movementInput != Vector3.zero) 
        {
            netVector3.data.Item1 = ID;

            netVector3.data.Item2 = movementInput;

            if (!NetworkManager.Instance.isServer)
            {
                NetworkManager.Instance.SendToServer(netVector3.Serialize());
            }
        }
    }

    public void Move(InputValue value)
    {
        if (ID == NetworkManager.Instance.playerData.ID)
        {
            movementInput = value.Get<Vector2>();
        }
    }
    
    public Vector2 GetPlayerPosition() 
    {
        return movementInput;
    }

    public void UpdatePlayerMovement(int id, Vector3 playerPos)
    {
        transform.position += playerPos * speed * Time.deltaTime;

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