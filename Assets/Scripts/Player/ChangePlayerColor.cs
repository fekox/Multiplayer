using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePlayerColor : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerColor;

    private Color newColor;

    void Start()
    {
        int randColor = Random.Range(1, 7);

        switch (randColor)
        {
            case 1:
                newColor = Color.blue;
                break;

            case 2:
                newColor = Color.red;
                break;

            case 3:
                newColor = Color.cyan;
                break;

            case 4:
                newColor = Color.green;
                break;

            case 5:
                newColor = Color.gray;
                break;

            case 6:
                newColor = Color.yellow;
                break;

            case 7:
                newColor = Color.magenta;
                break;
        }

        playerColor.color = newColor;
    }
}
