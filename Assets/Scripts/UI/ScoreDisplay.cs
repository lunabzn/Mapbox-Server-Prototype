using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Text scoreText; // Referencia al texto donde mostrar la puntuaci�n
    int score;
    public Text badgeText;
    [SerializeField] public GameObject badge1;
    [SerializeField] public GameObject panelBadge;
    [SerializeField] public GameObject menuPanel;

    // Cambiar firstEvent a privado y manejarlo con PlayerPrefs
    private bool firstEvent;

    void Start()
    {
        // Recuperar la puntuaci�n guardada en PlayerPrefs
        score = PlayerPrefs.GetInt("Score", 0);

        // Recuperar el estado de firstEvent desde PlayerPrefs
        firstEvent = PlayerPrefs.GetInt("FirstEvent", 0) == 1;

        // Mostrar la puntuaci�n en el texto
        scoreText.text = "                    " + score;

        // Solo mostrar el badge si es la primera vez y la puntuaci�n es 10
        if (score >= 10 && !firstEvent)
        {
            firstEvent = true;
            panelBadge.SetActive(true);
            badgeText.text = "�Enhorabuena por tu primer evento realizado!";

            // Guardar el estado de firstEvent en PlayerPrefs
            PlayerPrefs.SetInt("FirstEvent", 1);
        }
    }

    void Update()
    {
        // Verificar si el panel del men� est� activo y actualizar el badge
        if (menuPanel.activeSelf)
        {
            Logro1();
        }
    }

    public void Logro1()
    {
        if (score >= 10)
        {
            badge1.SetActive(true);
        }
    }

    private void OnApplicationQuit()
    {
        // Resetear la puntuaci�n y el estado de firstEvent cuando la aplicaci�n se cierre
        PlayerPrefs.DeleteKey("Score");
        PlayerPrefs.DeleteKey("FirstEvent");
    }
}
