using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private TextMeshProUGUI timerText;

    private float timerSeg = 240;

    private int seconds;
    private int minutes;

    private bool startGame = true;

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
}
