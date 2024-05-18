using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ErrorPopUp : MonoBehaviour
{
    [SerializeField] private string menuName = "Menu";

    public void OnErrorPopUpClick() 
    {
        SceneManager.LoadScene(menuName);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}
