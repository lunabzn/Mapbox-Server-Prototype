using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] public GameObject eventPanel;
    [SerializeField] public GameObject completeEventPanel;
    [SerializeField] public GameObject closerPanel;
    [SerializeField] public EventManager eventManager;
    [SerializeField] public GameObject menuPanel;
    [SerializeField] public TMP_Text textNombre;

    public string eventName;

    int auxID;
    

    public void DisplayPanel(int eventID)
    {
        auxID = eventID;
        textNombre.text = eventName;
        eventPanel.SetActive(true);
    }

    public void DisplayCompletePanel()
    {
        completeEventPanel.SetActive(true);
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
