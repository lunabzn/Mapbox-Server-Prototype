using UnityEngine;
using UnityEngine.UI;
using Mapbox.Unity.Map;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Mapbox.Utils;
using TMPro;

public class CreateEvent : MonoBehaviour
{

    [SerializeField] GameObject fondo; 
    [SerializeField] GameObject puntuacion;
    [SerializeField] GameObject menuBoton;
    [SerializeField] GameObject botonAddEvent;
    [SerializeField] GameObject panelEventos;
    [SerializeField] GameObject panelInputFH;

    [SerializeField] Button botonPaseo;
    [SerializeField] Button botonCafe;
    [SerializeField] Button botonComer;
    [SerializeField] Button botonCartas;
    [SerializeField] Button botonPersonalizar;

    [SerializeField] TMP_InputField inputFecha;
    //[SerializeField] TMP_InputField inputHora;
    [SerializeField] TMP_Dropdown dropdownHoras;   
    [SerializeField] TMP_Dropdown dropdownMinutos;
    [SerializeField] Button botonConfirmar;

    [SerializeField]
    GameObject exitButton; // El botón para salir del modo de creación de eventos

    [SerializeField]
    AbstractMap _map; // Referencia al mapa

    bool isCreatingEvent = false;
    bool isCreatingPredet = false;
    string title;
    string description;
    string date = "";
    string time = "";
    Vector2d geoPos;
    Vector3 worldPos;

    [SerializeField]
    RealTimeEventSpawner eventSpawner; // Script que spawnea eventos a tiempo real

    void Start()
    {
        exitButton.SetActive(false); // El botón de salir está oculto al inicio
        botonPersonalizar.onClick.AddListener(() => 
        { 
            isCreatingEvent = true; 
            panelEventos.SetActive(false);
        });
        botonPaseo.onClick.AddListener(() =>
        {
            title = "Demos un paseo";
            description = "Al terminar las clases nos reuniremos en la puerta de entrada a la universidad para dar un paseo por los alrededores";
            isCreatingPredet = true;
            panelEventos.SetActive(false);
        }) ;
        botonCafe.onClick.AddListener(() =>
        {
            title = "Tomemos un cafe";
            description = "Al terminar las clases nos reuniremos en la cafetería para tomarnos un café y charlar un rato";
            isCreatingPredet = true;
            panelEventos.SetActive(false);
        });
        botonComer.onClick.AddListener(() =>
        {
            title = "Comamos juntos";
            description = "Al terminar las clases nos reuniremos en la cafetería para almorzar y charlar un rato";
            isCreatingPredet = true;
            panelEventos.SetActive(false);
        });
        botonCartas.onClick.AddListener(() =>
        {
            title = "Juguemos una partida de cartas";
            description = "Al terminar las clases nos reuniremos en la cafetería para jugar una partida de cartas en las mesas libres";
            isCreatingPredet = true;
            panelEventos.SetActive(false);
        });
    }

    void Update()
    {
        eventSpawner = FindAnyObjectByType<RealTimeEventSpawner>();
        if (isCreatingEvent || isCreatingPredet)
        {
            HandleMapClick();
        }
    }

    public void OnAddEventButtonClicked()
    {        
        fondo.SetActive(false); // Oculta el Canvas principal
        puntuacion.SetActive(false);
        menuBoton.SetActive(false);
        botonAddEvent.SetActive(false);
        exitButton.SetActive(true); // Muestra el botón de salir
        panelEventos.SetActive(true);
    }

    public void OnExitButtonClicked()
    {
        isCreatingEvent = false;
        isCreatingPredet = false;
        fondo.SetActive(true); // Oculta el Canvas principal
        puntuacion.SetActive(true);
        botonAddEvent.SetActive(true);
        menuBoton.SetActive(true);  // Muestra el Canvas principal
        exitButton.SetActive(false); // Oculta el botón de salir
        panelEventos.SetActive(false);
    }

    public void OnConfirmButtonClicked()
    {
        panelInputFH.SetActive(false);
        date = inputFecha.text;
        int horas = int.Parse(dropdownHoras.options[dropdownHoras.value].text);
        int minutos = int.Parse(dropdownMinutos.options[dropdownMinutos.value].text);
        time = $"{horas:D2}:{minutos:D2}";
        Debug.Log($"LA HORA ES {time}");
        PredetEvent();
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
                worldPos = worldPosition;

                // Convertir worldPosition a coordenadas geográficas (latitud y longitud)
                Vector2d geoPosition = _map.WorldToGeoPosition(worldPosition);
                //Debug.Log("Geo Position: " + geoPosition);
                geoPos = geoPosition;

                // Solicitar al servidor crear el evento
                if (isCreatingEvent)
                {
                    // Solicitar al servidor crear el evento
                    PersonalizedEvent(worldPosition, geoPosition);
                }
                else if(isCreatingPredet)
                {
                    panelInputFH.SetActive(true);                    
                }

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
                    worldPos = worldPosition;

                    Vector2d geoPosition = _map.WorldToGeoPosition(worldPosition);
                    // Debug.Log("Geo Position: " + geoPosition);
                    geoPos = geoPosition;

                    if (isCreatingEvent)
                    {
                        // Solicitar al servidor crear el evento
                        PersonalizedEvent(worldPosition, geoPosition);
                    }
                    else if (isCreatingPredet)
                    {
                        panelInputFH.SetActive(true);
                    }

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


    public void PersonalizedEvent(Vector3 worldPos, Vector2d geoPos) 
    {
        eventSpawner.RequestCreateEvent(worldPos, geoPos);
    }

    public void PredetEvent()
    {
        eventSpawner.RequestCreatePredetEvent(worldPos, geoPos, title, description, date, time);
        Debug.Log("creando evento predet...");
    }
}
