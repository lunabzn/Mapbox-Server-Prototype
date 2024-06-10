using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventUIManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Comenzar()
    {
        SceneManager.LoadScene("Location-basedGame");
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene("Location-basedGame");
    }

    public void DoAct()
    {
        SceneManager.LoadScene("EventoRealizado");
    }

    public void ScanEvent()
    {
        SceneManager.LoadScene("QRCodeScanner");
    }
}


