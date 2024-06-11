using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class EventManager : MonoBehaviour
{
    public int maxDist = 100;
    void Start()
    {
        
    }
        
    void Update()
    {
        
    }

    public void JoinEventScene(int eventID)
    {
        if (eventID == 1) 
        {
            SceneManager.LoadScene("EventoAulario 2");
        } else if (eventID == 2)
        {
            SceneManager.LoadScene("EventoAulario 1");
        } else if (eventID == 3)
        {
            SceneManager.LoadScene("EventoCafeteria");
        }
        else if (eventID == 4)
        {
            SceneManager.LoadScene("EventoSecretaría");
        }
        else if (eventID == 5)
        {
            SceneManager.LoadScene("EventoPingPong");
        }
        else if (eventID == 6)
        {
            SceneManager.LoadScene("EventoBiblioteca");
        }
        else if (eventID == 7)
        {
            SceneManager.LoadScene("EventoBiblioteca");
        }
        else if (eventID == 8)
        {
            SceneManager.LoadScene("EventoAulario 2");
        }

    }
}
