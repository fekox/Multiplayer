using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] private string gameName = "Game";
    public void OnPlayClick() 
    {
        SceneManager.LoadScene(gameName);
    }

    public void OnQuitClick()
    {
        Debug.Log("QuitGame");
        Application.Quit();
    }
}
