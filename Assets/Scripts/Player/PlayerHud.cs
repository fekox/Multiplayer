using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;

    private string playerNameText = "none";

    public void SetPlayerName(string playerName) 
    {
        playerNameText = playerName;
    }

    private void Update()
    {
        playerName.text = playerNameText;
    }
}
