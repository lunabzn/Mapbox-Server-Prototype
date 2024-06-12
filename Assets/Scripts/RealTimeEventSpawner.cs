using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Mapbox.Utils;
using UnityEngine.UI;
using TMPro;

public class RealTimeEventSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject eventPrefab; // Prefab del evento a instanciar

    public GameObject mainCanvas;

    public List<GameObject> spawnedEvents = new List<GameObject>();
    public List<Vector3> eventPositions = new List<Vector3>(); 
    public List<Vector2d> geoPositions = new List<Vector2d>();

    public static RealTimeEventSpawner Instance { get; private set; }

    private void Awake()
    {
        mainCanvas = GameObject.Find("Canvas");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        

        for (int i = spawnedEvents.Count - 1; i >= 0; i--)
        {
            if (spawnedEvents[i] == null)
            {
                spawnedEvents.RemoveAt(i);
            }
            else
            {
                Vector3 currentPosition = spawnedEvents[i].transform.position;
                if (currentPosition.y != 5)
                {
                    Debug.LogWarning("Correcting Y position for: " + spawnedEvents[i].name);
                    spawnedEvents[i].transform.position = new Vector3(currentPosition.x, 5, currentPosition.z);
                }

                if (spawnedEvents[i].transform.hasChanged)
                {
                    Debug.LogWarning("Transform has changed for event: " + spawnedEvents[i].name);
                    spawnedEvents[i].transform.hasChanged = false;
                }
            }
        }
    }

    private void OnEnable()
    {
        // Suscribirse al evento SceneManager.sceneLoaded cuando este script se active
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Desuscribirse del evento SceneManager.sceneLoaded cuando este script se desactive
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (IsServer)
        {
            // Recrear los pines en el Start si es el servidor
            RecreateEvents();
        }
    }

    // M�todo llamado por los clientes para solicitar la creaci�n de un evento
    public void RequestCreateEvent(Vector3 worldPosition, Vector2d geoPosition)
    {
        if (IsClient)
        {
            // El cliente solicita la creaci�n del evento al servidor
            CreateEventServerRpc(worldPosition, geoPosition);
        }
    }

    // ServerRpc para manejar la solicitud de creaci�n de evento desde el cliente
    [ServerRpc(RequireOwnership = false)]
    public void CreateEventServerRpc(Vector3 worldPosition, Vector2d geoPosition, ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            // Comprobar si la ubicaci�n ya est� en la lista de eventos
            if (!eventPositions.Contains(worldPosition))
            {
                // A�adir la ubicaci�n del evento a la lista
                eventPositions.Add(worldPosition);
                geoPositions.Add(geoPosition);
                // Crear el evento en el servidor
                CreateEvent(worldPosition, geoPosition);
                // Notificar a todos los clientes para crear el evento en sus mapas
                CreateEventClientRpc(worldPosition, geoPosition);
            }
        }
    }

    // M�todo que crea el evento en el servidor
    void CreateEvent(Vector3 worldPosition, Vector2d geoPosition)
    {
        Debug.Log("Creating event at position: " + worldPosition);
        worldPosition.y = 5; // Establecer la Y en 5
        GameObject eventInstance = Instantiate(eventPrefab, worldPosition, Quaternion.identity);

        NetworkObject networkObject = eventInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }

        // Asignar coordenadas geogr�ficas al EventController del evento
        EventController eventController = eventInstance.GetComponent<EventController>();
        if (eventController != null)
        {
            eventController.isRealTime = true;
            eventController.eventLoc = geoPosition;
        }

        spawnedEvents.Add(eventInstance);

        // Asegurarse de que la posici�n Y no cambie despu�s de la instanciaci�n
        eventInstance.transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);

        // Mostrar el panel de entrada de datos
        mainCanvas.SetActive(false);
        ShowInputPanel(eventInstance);
    }

    // M�todo que activa el panel de entrada de datos en el evento
    void ShowInputPanel(GameObject eventInstance)
    {
        Transform canvas = eventInstance.transform.Find("EventCanvas");

        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);

            Transform inputPanel = canvas.Find("InputPanel");
            if (inputPanel != null)
            {
                inputPanel.gameObject.SetActive(true);

                // Obtener referencias a los InputField y Button
                // Obtener referencias a los InputField y Button
                TMP_InputField titleInput = inputPanel.Find("TitleInputField").GetComponent<TMP_InputField>();
                TMP_InputField infoInput = inputPanel.Find("InfoInputField").GetComponent<TMP_InputField>();
                Button confirmButton = inputPanel.Find("ConfirmButton").GetComponent<Button>();

                // Asignar el evento al bot�n para confirmar los datos
                confirmButton.onClick.AddListener(() => OnConfirmInput(eventInstance, titleInput.text, infoInput.text));
            }
        }
    }

    // M�todo para manejar la confirmaci�n de los datos de entrada
    void OnConfirmInput(GameObject eventInstance, string title, string info)
    {
        // Desactivar el InputPanel y el Canvas despu�s de la confirmaci�n
        Transform canvas = eventInstance.transform.Find("EventCanvas");
        if (canvas != null)
        {
            Transform inputPanel = canvas.Find("InputPanel");
            if (inputPanel != null)
            {
                inputPanel.gameObject.SetActive(false);
            }
            canvas.gameObject.SetActive(false);
        }

        // Enviar los datos al servidor para sincronizarlos con todos los clientes
        ulong objectId = eventInstance.GetComponent<NetworkObject>().NetworkObjectId;
        UpdateEventDataServerRpc(objectId, title, info);

        mainCanvas.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateEventDataServerRpc(ulong objectId, string title, string info)
    {
        GameObject eventInstance = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].gameObject;

        Transform canvas = eventInstance.transform.Find("EventCanvas");
        if (canvas != null)
        {
            Transform eventInfoPanel = canvas.Find("EventInfo");
            if (eventInfoPanel != null)
            {
                TextMeshProUGUI titleText = eventInfoPanel.Find("EventTitle").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI infoText = eventInfoPanel.Find("EventInstructions").GetComponent<TextMeshProUGUI>();

                if (titleText != null)
                {
                    titleText.text = title;
                }

                if (infoText != null)
                {
                    infoText.text = info;
                }
            }
        }


        // Notificar a todos los clientes para actualizar los datos del evento
        UpdateEventDataClientRpc(objectId, title, info);
    }

    // ClientRpc para actualizar los datos del evento en todos los clientes
    [ClientRpc]
    void UpdateEventDataClientRpc(ulong objectId, string title, string info)
    {
        GameObject eventInstance = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].gameObject;

        Transform canvas = eventInstance.transform.Find("EventCanvas");
        if (canvas != null)
        {
            Transform eventInfoPanel = canvas.Find("EventInfo");
            if (eventInfoPanel != null)
            {
                TextMeshProUGUI titleText = eventInfoPanel.Find("EventTitle").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI infoText = eventInfoPanel.Find("EventInstructions").GetComponent<TextMeshProUGUI>();

                if (titleText != null)
                {
                    titleText.text = title;
                }

                if (infoText != null)
                {
                    infoText.text = info;
                }
            }
        }
    }

    // M�todo llamado por el servidor para notificar a todos los clientes de la creaci�n del evento
    [ClientRpc]
    void CreateEventClientRpc(Vector3 worldPosition, Vector2d geoPosition)
    {
        if (!IsServer)
        {
            // Solo los clientes ejecutan este c�digo
            CreateClientEvent(worldPosition, geoPosition);
        }
    }

    // M�todo que crea el evento en el cliente (sin NetworkObject)
     void CreateClientEvent(Vector3 worldPosition, Vector2d geoPosition)
     {
        Debug.Log("Creating client-side event at position: " + worldPosition);
        worldPosition.y = 5; // Establecer la Y en 5
        GameObject eventInstance = Instantiate(eventPrefab, worldPosition, Quaternion.identity);

        // Asignar coordenadas geogr�ficas al EventController del evento
        EventController eventController = eventInstance.GetComponent<EventController>();

        if (eventController != null)
        {
            eventController.isRealTime = true;
            eventController.eventLoc = geoPosition;
        }

        spawnedEvents.Add(eventInstance);
        eventInstance.transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);
     }


    // M�todo para manejar la carga de escenas
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Location-basedGame") // Aseg�rate de que este es el nombre correcto de tu escena
        {
            mainCanvas = GameObject.Find("Canvas");

            Debug.Log("VOLVIMOS ESCENA LOCATION");
            // Activar el spawner y recrear los eventos al cargar la escena original
            gameObject.SetActive(true);

            // Instancia de nuevo los pines
            if (IsServer)
            {
                RecreateEvents();
            }
            else
            {
                RequestEventPositionsServerRpc();
            }
        }
    }

    private void RecreateEvents()
    {
        // Limpiar la lista de objetos instanciados
        foreach (var spawnedEvent in spawnedEvents)
        {
            if (spawnedEvent != null)
            {
                Destroy(spawnedEvent);
            }
        }
        spawnedEvents.Clear();

        if (eventPositions.Count == 0)
        {
            Debug.Log("No hay eventos para recrear.");
            return;
        }

        for (int i = 0; i < eventPositions.Count; i++)
        {
            Vector3 worldPosition = eventPositions[i];
            Vector2d geoPosition = geoPositions[i];

            Debug.Log("Recreating event at position: " + worldPosition);
            // Establecer la Y en 5
            Vector3 adjustedPosition = new Vector3(worldPosition.x, 5, worldPosition.z);

            // Instanciar el evento en la posici�n calculada
            GameObject eventInstance = Instantiate(eventPrefab, adjustedPosition, Quaternion.identity);
            spawnedEvents.Add(eventInstance);

            NetworkObject networkObject = eventInstance.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }

            EventController eventController = eventInstance.GetComponent<EventController>();
            if (eventController != null)
            {
                eventController.isRealTime = true;
                eventController.eventLoc = geoPosition;
            }

            eventInstance.transform.position = adjustedPosition;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestEventPositionsServerRpc(ServerRpcParams rpcParams = default)
    {
        // Enviar las posiciones de los eventos al cliente que lo solicit�
        SyncEventPositionsClientRpc(eventPositions.ToArray(), geoPositions.ToArray(), rpcParams.Receive.SenderClientId);
    }

   [ClientRpc]
    private void SyncEventPositionsClientRpc(Vector3[] positions, Vector2d[] geoPos, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            eventPositions.Clear();
            geoPositions.Clear();

            eventPositions.AddRange(positions);
            this.geoPositions.AddRange(geoPos);
            RecreateClientEvents();
        }
    }

    private void RecreateClientEvents()
    {
        if (eventPositions.Count == 0)
        {
            Debug.Log("No hay eventos para recrear.");
            return;
        }

        for (int i = 0; i < eventPositions.Count; i++)
        {
            Vector3 worldPosition = eventPositions[i];
            Vector2d geoPosition = geoPositions[i];

            Debug.Log("Recreating client-side event at position: " + worldPosition);
            // Establecer la Y en 5
            Vector3 adjustedPosition = new Vector3(worldPosition.x, 5, worldPosition.z);

            // Instanciar el evento en la posici�n calculada
            GameObject eventInstance = Instantiate(eventPrefab, adjustedPosition, Quaternion.identity);
            spawnedEvents.Add(eventInstance);

            EventController eventController = eventInstance.GetComponent<EventController>();
            if (eventController != null)
            {
                eventController.isRealTime = true;
                eventController.eventLoc = geoPosition;
            }

            eventInstance.transform.position = adjustedPosition;
        }
    }
}
