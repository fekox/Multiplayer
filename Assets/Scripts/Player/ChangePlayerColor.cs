using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePlayerColor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerColor;

    private Color newColor;

    void Start()
    {
        int randColor = Random.Range(1, 8);

        switch (randColor)
        {
            case 1:
                newColor = Color.blue;
                break;

            case 2:
                newColor = Color.red;
                break;

            case 3:
                newColor = Color.black;
                break;

            case 4:
                newColor = Color.cyan;
                break;

            case 5:
                newColor = Color.green;
                break;

            case 6:
                newColor = Color.gray;
                break;

            case 7:
                newColor = Color.yellow;
                break;

            case 8:
                newColor = Color.magenta;
                break;
        }

        playerColor.color = newColor;
    }
}
