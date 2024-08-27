using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] public GameObject eventPanel; 
    [SerializeField] public GameObject closerPanel;
    [SerializeField] public EventManager eventManager;
    [SerializeField] public GameObject menuPanel;

    int auxID;
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    public void DisplayPanel(int eventID)
    {
        auxID = eventID;
        eventPanel.SetActive(true);
    }
    public void GetCloserPanel()
    {
        closerPanel.SetActive(true);
    }

    public void CloseEventPanel()
    {
        eventPanel.SetActive(false);
    }

    public void CloseCloserPanel()
    {
        closerPanel.SetActive(false);
    }

    public void JoinButton()
    {
        eventManager.JoinEventScene(auxID);
    }

    public void ActiveMenu()
    {
        menuPanel.SetActive(true);
    }
    public void LeaveMenu()
    {
        menuPanel.SetActive(false);
    }
}
