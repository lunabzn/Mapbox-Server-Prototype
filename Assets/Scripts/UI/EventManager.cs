using Mapbox.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class EventManager : MonoBehaviour
{
    public int maxDist = 300;

    public void JoinEventScene(int eventID)
    {
        if (eventID == 1)
        {
            var spawner = FindObjectOfType<SpawnOnMap>();
            if (spawner != null) { spawner.OnFirstPinInteract(); }
            SceneManager.LoadScene("EspacioSeguro");
        }
        else if (eventID == 2) 
        {
            SceneManager.LoadScene("EventoCharla");
        } else if (eventID == 3)
        {
            SceneManager.LoadScene("EventoAulario 1");
        } else if (eventID == 4)
        {
            SceneManager.LoadScene("EventoCafeteria");
        }
        else if (eventID == 5)
        {
            SceneManager.LoadScene("EventoSecretaría");
        }
        else if (eventID == 6)
        {
            SceneManager.LoadScene("EventoPingPong");
        }
        else if (eventID == 7)
        {
            SceneManager.LoadScene("EventoBiblioteca");
        }
        else if (eventID == 8)
        {
            SceneManager.LoadScene("EventoBiblioteca");
        }

    }

    public string eventName(int id)
    {
        switch (id)
        {
            case 1: return "Espacio seguro";
            case 2: return "Charla";
            case 3: return "Aulario 1";
            case 4: return "Cafeteria";
            case 5: return "Secretaría";
            case 6: return "Ping Pong";
            case 7: return "Biblioteca";
            case 8: return "Biblioteca";
            default: return "Evento desconocido";
        }        
    }
}
