using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class ScoreDisplay : MonoBehaviour
{
    public Text scoreText; // Referencia al texto donde mostrar la puntuación
    int score;
    public Text badgeText;
    [SerializeField] public GameObject badge1;
    [SerializeField] public GameObject panelBadge;
    [SerializeField] public GameObject menuPanel;

    void Start()
    {
        // Recuperar la puntuación guardada en PlayerPrefs
        score = PlayerPrefs.GetInt("Score", 0);

        // Mostrar la puntuación en el texto
        scoreText.text = "           " + score;

        if (score == 10)
        {
            panelBadge.SetActive(true);
            badgeText.text = " Enhorabuena por tu primero evento realizado! ";
        }
    }
    void Update()
    {
        // Check if the menu panel is active and update the badge
        if (menuPanel.activeSelf)
        {
            logro1();
        }
    }

    public void logro1()
    {
        if (score>= 10)
        {
            badge1.SetActive(true);
        }
    }

    private void OnApplicationQuit()
    {
        // Reset the score when the application is quit
        PlayerPrefs.DeleteKey("Score");
    }
}
