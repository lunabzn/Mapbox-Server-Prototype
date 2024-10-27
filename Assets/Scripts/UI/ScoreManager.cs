using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    //public Text scoreText;

    private int score;

    private void Start()
    {
        score = PlayerPrefs.GetInt("Score", 0);
    }

    public void AddHalfScore()
    {
       // la mitad de puntuacion para eventos empezados pero no terminados
        score += 5;
        PlayerPrefs.SetInt("Score", score);
    }
    public void AddFullScore()
    {
        // la mitad de puntuacion para eventos empezados pero no terminados
        score += 10;
        PlayerPrefs.SetInt("Score", score);
    }

    private void OnApplicationQuit()
    {
        // Resetear player prefs
        PlayerPrefs.DeleteKey("Score");
    }

   
}