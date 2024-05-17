using System.Collections;
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

    [Header("Setup")]

    private float timerSeg = 240;

    private int seconds;
    private int minutes;

    private bool startGame = true;

    public int currentPlayers;

    private void Start()
    {
        if (currentPlayers > 0) 
        {
            SpawnPlayer();
        }
    }

    void Update()
    {
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

    public void SpawnPlayer() 
    {
        for (int i = 0; i < currentPlayers; i++) 
        {
            Instantiate(playerPrefab, spawns[i].transform.position, spawns[i].transform.rotation);
        }
    }
}
