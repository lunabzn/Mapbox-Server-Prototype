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
    public bool isRealTime;

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

    //void pinPosition() { transform.position = new Vector3(transform.position.x, 5, transform.position.z); }
    void pinPosition()
    {
        // Obtener la referencia al AbstractMap en la escena
        var map = FindObjectOfType<Mapbox.Unity.Map.AbstractMap>();

        // Convertir la ubicación del evento (coordenadas geográficas) a una posición en el mundo
        Vector3 worldPosition = map.GeoToWorldPosition(eventLoc);

        // Establecer la posición del pin en la posición calculada en el mundo
        transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);
    }


    public void OnMouseDown()
    {
        playerLoc = GameObject.Find("Canvas").GetComponent<LocationStatus>();
        var currentPlayerLoc = new GeoCoordinatePortable.GeoCoordinate(playerLoc.GetLat(), playerLoc.GetLon());
        var currentEventLoc = new GeoCoordinatePortable.GeoCoordinate(eventLoc.x, eventLoc.y);
        var distance = currentPlayerLoc.GetDistanceTo(currentEventLoc);
        Debug.Log(distance);

        // Verificar si el evento ya está completado usando PlayerPrefs
        bool isEventCompleted = PlayerPrefs.GetInt("EventCompleted_" + (eventID - 1), 0) == 1;

        if (isEventCompleted)
        {
            // Si el evento ya está completado, mostrar el panel indicando que ya está hecho
            uiManager.DisplayCompletePanel();
            return;
        }

        if (!isRealTime)
        {           
            if (distance < eventManager.maxDist) 
            {
                uiManager.eventName = eventManager.eventName(eventID);
                uiManager.DisplayPanel(eventID); 
            }
            if (distance > eventManager.maxDist) { uiManager.GetCloserPanel(); }
        } 
        else if(isRealTime)
        {
            if (distance < eventManager.maxDist) 
            { 
                GameObject eventCanvas = transform.Find("EventCanvas").gameObject;
                eventCanvas.SetActive(true);

                if(eventCanvas!= null)
                {
                    GameObject infoPanel = eventCanvas.transform.Find("EventInfo").gameObject;
                    infoPanel.SetActive(true);
                    // Obtener el índice del evento en la lista
                    int index = RealTimeEventSpawner.Instance.spawnedEvents.IndexOf(gameObject);
                    if (index >= 0 && RealTimeEventSpawner.Instance.eventInfoDictionary.TryGetValue(index, out var eventData))
                    {
                        // Actualizar el panel de información con los datos
                        RealTimeEventSpawner.Instance.UpdateEventInfoPanel(gameObject, eventData.title, eventData.info, eventData.date, eventData.time);
                    }
                }
            }
            if (distance > eventManager.maxDist) { uiManager.GetCloserPanel(); }
        }        

    }
}
