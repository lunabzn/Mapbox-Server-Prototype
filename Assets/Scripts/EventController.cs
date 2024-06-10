using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Examples;
using Mapbox.Utils;
using Unity.VisualScripting;

public class EventController : MonoBehaviour
{
    public static Canvas canvas;
    LocationStatus playerLoc;
    public Vector2d eventLoc;
    public int eventID;
    UIManager uiManager;
    EventManager eventManager;
    void Start()
    {
        if (canvas == null)
        {
            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }

        uiManager = canvas.GetComponent<UIManager>();
        eventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
    }

    void Update()
    {
        pinPosition();
    }

    void pinPosition() { transform.position = new Vector3(transform.position.x, 5, transform.position.z); }
   

    public void OnMouseDown()
    {
        playerLoc = GameObject.Find("Canvas").GetComponent<LocationStatus>();
        var currentPlayerLoc = new GeoCoordinatePortable.GeoCoordinate(playerLoc.GetLat(), playerLoc.GetLon());
        var currentEventLoc = new GeoCoordinatePortable.GeoCoordinate(eventLoc[0], eventLoc[1]);
        var distance = currentPlayerLoc.GetDistanceTo(currentEventLoc);
        Debug.Log(distance);

        //Debug.Log("click"); 

        if (distance < eventManager.maxDist) { uiManager.DisplayPanel(eventID); }
        if (distance > eventManager.maxDist) { uiManager.GetCloserPanel(); }
    }
}
