using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public Text scoreText;
    public Button addScoreButton;

    private int score;

    private void Start()
    {
        // Load the score from PlayerPrefs
        score = PlayerPrefs.GetInt("Score", 0);
        UpdateScoreText();

        // Add a listener to the add score button
        addScoreButton.onClick.AddListener(AddScore);
    }

    private void AddScore()
    {
        // Add 10 points to the score
        score += 10;
        UpdateScoreText();

        // Save the score to PlayerPrefs
        PlayerPrefs.SetInt("Score", score);
    }

    private void UpdateScoreText()
    {
        // Update the score text
        scoreText.text = "Score: " + score;
    }

    private void OnApplicationQuit()
    {
        // Reset the score when the application is quit
        PlayerPrefs.DeleteKey("Score");
    }

    /*private void OnGUI()
    {
        // Display the score on the main menu screen
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            GUI.Label(new Rect(10, 10, 200, 50), "Score: " + score);
        }
    }*/
}