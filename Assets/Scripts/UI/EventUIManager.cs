using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventUIManager : MonoBehaviour
{
    public GameObject[] panels; // Array con todos los paneles de la escena
    public GameObject firstPanel; // Panel activo al inicio

    void Start()
    {
        ResetPanels();
    }

    // Este método desactiva todos los paneles excepto el primero
    void ResetPanels()
    {
        if (panels != null)
        {
            foreach (GameObject panel in panels)
            {
                panel.SetActive(false);
            }
            firstPanel.SetActive(true);

        }
    }

    public void SeleccionarAvatar()
    {
        SceneManager.LoadScene("AvatarSelection");
    }

    public void Comenzar()
    {
        SceneManager.LoadScene("Location-basedGame");
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("Location-basedGame");
    }

    public void ReturnToSafePlace()
    {
        SceneManager.LoadScene("EspacioSeguroBoton");
    }
    
    public void FeedbackScene()
    {
        SceneManager.LoadScene("FeedbackScene");
    }

    public void ScanEvent()
    {
        SceneManager.LoadScene("QRCodeScanner");
    }
}


