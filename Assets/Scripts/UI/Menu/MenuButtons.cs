using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private string gameName = "Game";
    public void OnClickPlay() 
    {
        SceneManager.LoadScene(gameName);
    }

    public void OnClickQuit()
    {
        Debug.Log("QuitGame");
        Application.Quit();
    }
}
