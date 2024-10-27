using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Text scoreText; // Referencia al texto donde mostrar la puntuación
    int score;
    public Text badgeText;
    [SerializeField] public GameObject badge1;
    [SerializeField] public GameObject badge2;
    [SerializeField] public GameObject badge3;
    [SerializeField] public GameObject panelBadge;
    [SerializeField] public GameObject menuPanel;

    private bool firstEvent = false;
    private bool threeEvents = false;
    private bool underTime = false;
    private bool logroTiempo = false;


    void Start()
    {
         // Recuperar la puntuación guardada en PlayerPrefs
        score = PlayerPrefs.GetInt("Score", 0);

        // Recuperar el estado de firstEvent desde PlayerPrefs
        if (PlayerPrefs.GetInt("FirstEvent") == 1) { firstEvent = true; };

        // Recuperar el estado de threeEvents desde PlayerPrefs
        if (PlayerPrefs.GetInt("ThreeEvents") == 1) { threeEvents = true; };

        // Recuperar el estado de threeEvents desde PlayerPrefs
        if (PlayerPrefs.GetInt("UnderTime") == 1) { underTime = true; };

        //Guardar estado logro del tiempo
        if(PlayerPrefs.GetInt("LogroTiempo") == 1) {  logroTiempo = true; };

        // Mostrar la puntuación en el texto
        scoreText.text = "                    " + score;

        // Solo mostrar el badge si es la primera vez y la puntuación es 10
        if (score >= 10 && !firstEvent)
        {
            firstEvent = true;
            panelBadge.SetActive(true);
            badgeText.text = "¡Enhorabuena por tu primer evento realizado!";

            // Guardar el estado de firstEvent en PlayerPrefs
            PlayerPrefs.SetInt("FirstEvent", 1);
        }
        else if (score >= 30 && !threeEvents)
        {
            threeEvents = true;
            panelBadge.SetActive(true);
            badgeText.text = "¡Bien! Llevas ya 3 eventos";

            // Guardar el estado de firstEvent en PlayerPrefs
            PlayerPrefs.SetInt("ThreeEvents", 1);
        }
        else if(underTime && !logroTiempo)
        {
            logroTiempo = true;
            panelBadge.SetActive(true);
            badgeText.text = "WOW ¡Conseguiste acabar un evento en menos de 10 minutos!";

            //Guardar logro tiempo
            PlayerPrefs.SetInt("LogroTiempo", 1);
        }
    }


    void Update()
    {
        // Verificar si el panel del menú está activo y actualizar el badge
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
        if (score >= 30)
        {
            badge2.SetActive(true);
        }
        if (underTime)
        {
            badge3.SetActive(true);
        }
    }

    public void volverInstrucciones()
    {
        SceneManager.LoadScene("Tutorial");
    }

    private void OnApplicationQuit()
    {
        // Resetear la puntuación y el estado de firstEvent cuando la aplicación se cierre
        PlayerPrefs.DeleteKey("Score");
        PlayerPrefs.DeleteKey("FirstEvent");
        PlayerPrefs.DeleteKey("UnderTime");
    }
}
