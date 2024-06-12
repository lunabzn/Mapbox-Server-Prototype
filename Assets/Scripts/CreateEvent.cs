using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Mapbox.Utils;

public class CreateEvent : MonoBehaviour
{
    [SerializeField]
    GameObject mainCanvas; // El Canvas principal con todos los elementos de la UI

    [SerializeField]
    GameObject exitButton; // El botón para salir del modo de creación de eventos

    [SerializeField]
    AbstractMap _map; // Referencia al mapa

    bool isCreatingEvent = false;

    [SerializeField]
    RealTimeEventSpawner eventSpawner; // Script que spawnea eventos a tiempo real

    void Start()
    {
        exitButton.SetActive(false); // El botón de salir está oculto al inicio
    }

    void Update()
    {
        eventSpawner = FindAnyObjectByType<RealTimeEventSpawner>();
        if (isCreatingEvent)
        {
            HandleMapClick();
        }
    }

    public void OnAddEventButtonClicked()
    {
        isCreatingEvent = true;
        //Debug.Log("isCreatingEvent: " + isCreatingEvent);
        mainCanvas.SetActive(false); // Oculta el Canvas principal
        exitButton.SetActive(true); // Muestra el botón de salir
    }

    public void OnExitButtonClicked()
    {
        isCreatingEvent = false;
        //Debug.Log("isCreatingEvent: " + isCreatingEvent);
        mainCanvas.SetActive(true); // Muestra el Canvas principal
        exitButton.SetActive(false); // Oculta el botón de salir
    }

    void HandleMapClick()
    {
        // Detectar clic del ratón en PC
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject(Input.mousePosition))
        {
            Vector3 mousePosition = Input.mousePosition;

            // Ajustar la distancia de la cámara al plano del suelo
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            Plane plane = new Plane(Vector3.up, 0);
            float distance = 0;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);
                //Debug.Log("World Position: " + worldPosition);

                // Convertir worldPosition a coordenadas geográficas (latitud y longitud)
                Vector2d geoPosition = _map.WorldToGeoPosition(worldPosition);
                //Debug.Log("Geo Position: " + geoPosition);

                // Solicitar al servidor crear el evento
                eventSpawner.RequestCreateEvent(worldPosition, geoPosition);

                OnExitButtonClicked(); // Salir del modo de creación de eventos después de crear el evento
            }
        }

        // Detectar toques en dispositivos móviles
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            if (!IsPointerOverUIObject(touch.position))
            {
                Vector3 touchPosition = touch.position;

                // Ajustar la distancia de la cámara al plano del suelo
                Ray ray = Camera.main.ScreenPointToRay(touchPosition);
                Plane plane = new Plane(Vector3.up, 0);
                float distance = 0;
                if (plane.Raycast(ray, out distance))
                {
                    Vector3 worldPosition = ray.GetPoint(distance);
                    //Debug.Log("World Position: " + worldPosition);

                    Vector2d geoPosition = _map.WorldToGeoPosition(worldPosition);
                   // Debug.Log("Geo Position: " + geoPosition);

                    // Solicitar al servidor crear el evento
                    eventSpawner.RequestCreateEvent(worldPosition, geoPosition);

                    OnExitButtonClicked(); // Salir del modo de creación de eventos después de crear el evento
                }
            }
        }
    }

    // Método para verificar si el toque/clic es sobre un UI
    private bool IsPointerOverUIObject(Vector2 position)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = position
        }; // Inicializa el puntero con el EventSystem de la escena
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results); // Hace un raycast y llena la lista con elementos de la UI bajo puntero

        foreach (var result in results)
        {
            Debug.Log("UI element hit: " + result.gameObject.name);
        }

        return results.Count > 0;
    }
}
