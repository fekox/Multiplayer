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

    private int spawn;

    public List<GameObject> playersGO;

    private void Start()
    {
        playersGO.Clear();
    }

    public void SpawnPlayer(string playerName) 
    {
        playerNameInstance = Instantiate(playerPrefab, spawns[spawn].transform.position, spawns[spawn].transform.rotation);

        spawn++;

        playersGO.Add(playerNameInstance);

        playerHud = playerNameInstance.GetComponent<PlayerHud>();

        playerHud.SetPlayerName(playerName);
    }

    public void SpawnPlayerFromClient(string playerName)
    {
        for (int i = 0; i < playersGO.Count; i++) 
        {
            if (playersGO[i].GetComponent<PlayerHud>().GetPlayerName() == playerName)
            {
                return;
            }
        }

        playerNameInstance = Instantiate(playerPrefab, spawns[spawn].transform.position, spawns[spawn].transform.rotation);

        spawn++;

        playersGO.Add(playerNameInstance);

        playerHud = playerNameInstance.GetComponent<PlayerHud>();

        playerHud.SetPlayerName(playerName);
    }

    public PlayerMovement GetPlayerByID(string name) 
    {
        for (int i = 0; i < playersGO.Count; i++)
        {
            Debug.Log($"player name: {playersGO[i].GetComponent<PlayerHud>().GetPlayerName()}");

            if (playersGO[i].GetComponent<PlayerHud>().GetPlayerName() == name)
            {
                return playersGO[i].GetComponent<PlayerMovement>();
            }
        }

        return null;
    }
}
