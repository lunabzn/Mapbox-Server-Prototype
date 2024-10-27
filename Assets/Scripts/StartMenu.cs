using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
  
    public void StartGame()
    {
        if(PlayerPrefs.GetInt("Tutorial") != 1)
        {
            SceneManager.LoadScene("Tutorial");
            PlayerPrefs.SetInt("Tutorial", 1);
        }
        else
        {
            SceneManager.LoadScene("Location-basedGame");
        }
    }
}
