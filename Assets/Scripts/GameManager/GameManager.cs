using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private List<GameObject> spawns;

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private NetworkManager networkManager;

    private GameObject playerNameInstance;

    private PlayerHud playerHud;

    public void SpawnPlayer(string playerName) 
    {
        int spawn = UnityEngine.Random.Range(0, 4);

        playerNameInstance = Instantiate(playerPrefab, spawns[spawn].transform.position, spawns[spawn].transform.rotation);

        playerHud = playerNameInstance.GetComponent<PlayerHud>();

        playerHud.SetPlayerName(playerName);
    }
}
