using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private List<GameObject> spawns;

    [SerializeField] private List<GameObject> playersGO;

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private NetworkManager networkManager;

    private GameObject playerNameInstance;

    private PlayerHud playerHud;

    private int spawn;

    public void SpawnPlayer(string playerName) 
    {
        playerNameInstance = Instantiate(playerPrefab, spawns[spawn].transform.position, spawns[spawn].transform.rotation);

        spawn++;

        playersGO.Add(playerPrefab);

        playerHud = playerNameInstance.GetComponent<PlayerHud>();

        playerHud.SetPlayerName(playerName);
    }
}
