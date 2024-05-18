using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private List<GameObject> spawns;

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private NetworkManager networkManager;

    private GameObject playerNameInstance;

    private PlayerHud playerHud;

    [Header("Setup")]

    [SerializeField] private bool startGame = false;

    private float timerSeg = 240;

    private int seconds;
    private int minutes;

    public void StartGame() 
    {
        startGame = true;

        GameTimer();
    }

    public void GameTimer() 
    {
        if (startGame)
        {
            int oneMinute = 60;

            timerSeg -= Time.deltaTime;

            seconds = (int)timerSeg % oneMinute;
            minutes = (int)timerSeg / oneMinute;

            timerText.text = string.Format("{00:00}:{1:00}", minutes, seconds);
        }

        if (timerSeg <= 0)
        {
            startGame = false;
        }
    }

    public void SpawnPlayer(string playerName) 
    {
        int spawn = UnityEngine.Random.Range(0, 4);

        playerNameInstance = Instantiate(playerPrefab, spawns[spawn].transform.position, spawns[spawn].transform.rotation);

        playerHud = playerNameInstance.GetComponent<PlayerHud>();

        playerHud.SetPlayerName(playerName);
    }
}
