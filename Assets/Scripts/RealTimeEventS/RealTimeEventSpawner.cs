using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Mapbox.Utils;
using UnityEngine.UI;
using TMPro;
using System;
using Mapbox.Unity.Map;

public class RealTimeEventSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject eventPrefab; // Prefab del evento a instanciar

    // Estructura para almacenar los datos del panel de información
    [Serializable]
    public class EventInfoData
    {
        public string title;
        public string info;
        public string date;
        public string time;
    }

    [SerializeField]
    public Dictionary<int, EventInfoData> eventInfoDictionary = new Dictionary<int, EventInfoData>();

    public GameObject mainCanvas;

    public List<GameObject> spawnedEvents = new List<GameObject>();
    public List<Vector3> eventPositions = new List<Vector3>();
    public List<Vector2d> geoPositions = new List<Vector2d>();
    private AbstractMap map; // Referencia al mapa

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

    private void Start()
    {
        if (IsServer)
        {
            // Recrear los pines en el Start si es el servidor
            RecreateEvents();
        }
    }

    private void Update()
    {

        if (map == null) return; // Salir si no hay referencia al mapa

        for (int i = spawnedEvents.Count - 1; i >= 0; i--)
        {
            var eventInstance = spawnedEvents[i];

            if (eventInstance == null)
            {
                spawnedEvents.RemoveAt(i); // Remover si es null
                continue;
            }

            // Asegurar que la altura Y sea 5
            var currentPosition = eventInstance.transform.position;
            if (currentPosition.y != 5)
            {
                eventInstance.transform.position = new Vector3(currentPosition.x, 5, currentPosition.z);
            }

            // Resetear hasChanged si ha cambiado
            if (eventInstance.transform.hasChanged)
            {
                eventInstance.transform.hasChanged = false;
            }

            // Asegurarse de que geoPositions tenga suficientes elementos
            if (i < geoPositions.Count)
            {
                // Actualizar la posición del evento
                UpdateEventPosition(eventInstance, geoPositions[i]);
            }
            else
            {
                Debug.LogWarning("Index out of range: " + i + ". geoPositions.Count: " + geoPositions.Count);
            }
        }

    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Location-basedGame")
        {
            mainCanvas = GameObject.Find("Canvas");
            map = FindObjectOfType<AbstractMap>();
            gameObject.SetActive(true);

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

    public void RequestCreatePredetEvent(Vector3 worldPos, Vector2d geoPos, string title, string description, string date, string time)
    {
        if (IsClient)
        {
            CreatePredetEventServerRpc(worldPos, geoPos, title, description, date, time, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreatePredetEventServerRpc(Vector3 worldPos, Vector2d geoPos, string title, string description, string date, string time, ulong clientId, ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            eventPositions.Add(worldPos);
            geoPositions.Add(geoPos);
            CreatePredetEvent(worldPos, geoPos, title, description, date, time, clientId);
            CreateEventClientRpc(worldPos, geoPos);
        }
    }

    public void RequestCreateEvent(Vector3 worldPosition, Vector2d geoPosition)
    {
        if (IsClient)
        {
            CreateEventServerRpc(worldPosition, geoPosition, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CreateEventServerRpc(Vector3 worldPosition, Vector2d geoPosition, ulong clientId, ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            if (!eventPositions.Contains(worldPosition))
            {
                eventPositions.Add(worldPosition);
                geoPositions.Add(geoPosition);
                CreateEvent(worldPosition, geoPosition, clientId);
                CreateEventClientRpc(worldPosition, geoPosition);
            }
        }
    }

    void CreatePredetEvent(Vector3 worldPosition, Vector2d geoPosition, string title, string description, string date, string time, ulong clientId)
    {
        worldPosition.y = 5;
        GameObject eventInstance = Instantiate(eventPrefab, worldPosition, Quaternion.identity);

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

        spawnedEvents.Add(eventInstance);
        eventInstance.transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);

        int index = spawnedEvents.IndexOf(eventInstance);
        Debug.Log("Index of created event: " + index);

        OnConfirmInput(eventInstance, title, description, date, time, index);

    }

    void CreateEvent(Vector3 worldPosition, Vector2d geoPosition, ulong clientId)
    {
        worldPosition.y = 5;
        GameObject eventInstance = Instantiate(eventPrefab, worldPosition, Quaternion.identity);

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

        spawnedEvents.Add(eventInstance);
        eventInstance.transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);

        int index = spawnedEvents.IndexOf(eventInstance);
        Debug.Log("Index of created event: " + index);

        ShowInputPanelClientRpc(clientId, networkObject.NetworkObjectId, index);
    }

    [ClientRpc]
    void ShowInputPanelClientRpc(ulong clientId, ulong eventNetworkObjectId, int index)
    {
        Debug.Log("Index of created event CLIENT: " + index);
        GameObject eventInstance = NetworkManager.Singleton.SpawnManager.SpawnedObjects[eventNetworkObjectId].gameObject;
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (eventInstance != null)
            {
                ShowInputPanel(eventInstance, index);
            }
        }
    }

    void ShowInputPanel(GameObject eventInstance, int index)
    {
        Transform canvas = eventInstance.transform.Find("EventCanvas");

        if (mainCanvas != null)
        {
            mainCanvas.SetActive(false);
        }

        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);

            Transform inputPanel = canvas.Find("InputPanel");
            if (inputPanel != null)
            {
                inputPanel.gameObject.SetActive(true);

                TMP_InputField titleInput = inputPanel.Find("TitleInputField").GetComponent<TMP_InputField>();
                TMP_InputField infoInput = inputPanel.Find("InfoInputField").GetComponent<TMP_InputField>();
                TMP_InputField dateInput = inputPanel.Find("DateInputField").GetComponent<TMP_InputField>();
                //TMP_InputField timeInput = inputPanel.Find("TimeInputField").GetComponent<TMP_InputField>();
                TMP_Dropdown dropDownHora = inputPanel.Find("DropdownHora").GetComponent<TMP_Dropdown>();
                TMP_Dropdown dropDownMin = inputPanel.Find("DropdownMinutos").GetComponent<TMP_Dropdown>();
                string time = $"{int.Parse(dropDownHora.options[dropDownHora.value].text):D2}:{int.Parse(dropDownMin.options[dropDownMin.value].text):D2}";
                Button confirmButton = inputPanel.Find("ConfirmButton").GetComponent<Button>();

                confirmButton.onClick.AddListener(() => OnConfirmInput(eventInstance, titleInput.text, infoInput.text, dateInput.text, time, index));
            }
        }
    }

    void OnConfirmInput(GameObject eventInstance, string title, string info, string date, string time, int index)
    {
        Transform canvas = eventInstance.transform.Find("EventCanvas");
        if (canvas != null)
        {
            Transform inputPanel = canvas.Find("InputPanel");
            if (inputPanel != null)
            {
                //Nos aseguramos de que si cambia el valor, se actualice el string
                TMP_Dropdown dropDownHora = inputPanel.Find("DropdownHora").GetComponent<TMP_Dropdown>();
                TMP_Dropdown dropDownMin = inputPanel.Find("DropdownMinutos").GetComponent<TMP_Dropdown>();
                time = $"{int.Parse(dropDownHora.options[dropDownHora.value].text):D2}:{int.Parse(dropDownMin.options[dropDownMin.value].text):D2}";
                inputPanel.gameObject.SetActive(false);
            }
            canvas.gameObject.SetActive(false);
        }

        if (index >= 0)
        {
            // Almacenar los datos en el diccionario localmente
            eventInfoDictionary[index] = new EventInfoData { title = title, info = info, date = date, time = time };
            Debug.Log(eventInfoDictionary[index].info + " DATOS GUARDADOS EN DICCIONARIO CON INDICE: " + index);

            // Actualizar en servidor
            UpdateEventDataServerRpc(eventInstance.GetComponent<NetworkObject>().NetworkObjectId, index, title, info, date, time);
        }

        if (mainCanvas != null)
        {
            mainCanvas.SetActive(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateEventDataServerRpc(ulong objectId, int index, string title, string info, string date, string time)
    {
        // Actualizar el diccionario con los nuevos datos en el servidor
        if (eventInfoDictionary.ContainsKey(index))
        {
            eventInfoDictionary[index] = new EventInfoData { title = title, info = info, date = date, time = time };
        }
        else
        {
            eventInfoDictionary.Add(index, new EventInfoData { title = title, info = info, date = date, time = time });
        }
        Debug.Log(eventInfoDictionary[index].info + " DATOS GUARDADOS EN DICCIONARIO SERVIDOR");
        PrintEventInfoDictionary();

        // Sincronizar la entrada específica del diccionario con todos los clientes
        SyncEventInfoEntryClientRpc(objectId, index, title, info, date, time);
    }

    [ClientRpc]
    void SyncEventInfoEntryClientRpc(ulong objectId, int index, string title, string info, string date, string time)
    {
        Debug.Log("ClientRpc received. Updating event info panel.");
        // Actualizar el diccionario en el cliente
        if (eventInfoDictionary.ContainsKey(index))
        {
            eventInfoDictionary[index] = new EventInfoData { title = title, info = info, date = date, time = time };
        }
        else
        {
            eventInfoDictionary.Add(index, new EventInfoData { title = title, info = info, date = date, time = time });
        }

        // Actualizar el panel de información si el objeto existe
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObject))
        {
            GameObject eventInstance = networkObject.gameObject;
            Debug.Log("Updating event info panel for event with index: " + index);
            UpdateEventInfoPanel(eventInstance, title, info, date, time);
        }
        else
        {
            Debug.LogWarning("Event instance not found in SyncEventInfoEntryClientRpc");
        }
    }

    public void UpdateEventInfoPanel(GameObject eventInstance, string title, string info, string date, string time)
    {
        Transform canvas = eventInstance.transform.Find("EventCanvas");
        if (canvas != null)
        {
            Transform eventInfoPanel = canvas.Find("EventInfo");
            if (eventInfoPanel != null)
            {
                Debug.Log("Actualizando el panel de información.");
                TextMeshProUGUI titleText = eventInfoPanel.Find("EventTitle").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI infoText = eventInfoPanel.Find("EventInstructions").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI dateText = eventInfoPanel.Find("EventDate").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI timeText = eventInfoPanel.Find("EventTime").GetComponent<TextMeshProUGUI>();

                if (titleText != null)
                {
                    titleText.SetText(title);
                    Debug.Log("TITULODEL PANEL: " + titleText.text);
                    titleText.ForceMeshUpdate();
                }

                if (infoText != null)
                {
                    infoText.SetText(info);
                    Debug.Log("INFO DEL PANEL: " + infoText.text);
                    infoText.ForceMeshUpdate();
                }
                if (dateText != null)
                {
                    dateText.SetText(date);
                    Debug.Log("TITULODEL PANEL: " + dateText.text);
                    dateText.ForceMeshUpdate();
                }

                if (timeText != null)
                {
                    timeText.SetText(time);
                    Debug.Log("INFO DEL PANEL: " + timeText.text);
                    timeText.ForceMeshUpdate();
                }
            }
        }
    }

    [ClientRpc]
    void CreateEventClientRpc(Vector3 worldPosition, Vector2d geoPosition)
    {
        if (!IsServer)
        {
            CreateClientEvent(worldPosition, geoPosition);
        }
    }

    void CreateClientEvent(Vector3 worldPosition, Vector2d geoPosition)
    {
        worldPosition.y = 5;
        GameObject eventInstance = Instantiate(eventPrefab, worldPosition, Quaternion.identity);

        EventController eventController = eventInstance.GetComponent<EventController>();

        if (eventController != null)
        {
            eventController.isRealTime = true;
            eventController.eventLoc = geoPosition;
        }

        spawnedEvents.Add(eventInstance);
        eventInstance.transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);
    }

    #region RECREACION DE EVENTOS

    //recrea los eventos al volver a la escena
    private void RecreateEvents()
    {
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

            Vector3 adjustedPosition = new Vector3(worldPosition.x, 5, worldPosition.z);

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

            ulong objectId = eventInstance.GetComponent<NetworkObject>().NetworkObjectId;
            Debug.Log($"ID del objeto recreado: {objectId}");
            if (eventInfoDictionary.ContainsKey(i))
            {
                EventInfoData eventData = eventInfoDictionary[i];
                Debug.Log("Cargando información del diccionario.");
                UpdateEventInfoPanel(eventInstance, eventData.title, eventData.info, eventData.date, eventData.time);
            }
        }

        PrintEventInfoDictionary();
    }

    //recrea los eventos al cliente
    [ServerRpc(RequireOwnership = false)]
    private void RequestEventPositionsServerRpc(ServerRpcParams rpcParams = default)
    {
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
            geoPositions.AddRange(geoPos);
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

            Vector3 adjustedPosition = new Vector3(worldPosition.x, 5, worldPosition.z);

            GameObject eventInstance = Instantiate(eventPrefab, adjustedPosition, Quaternion.identity);
            spawnedEvents.Add(eventInstance);

            EventController eventController = eventInstance.GetComponent<EventController>();
            if (eventController != null)
            {
                eventController.isRealTime = true;
                eventController.eventLoc = geoPosition;
            }

            eventInstance.transform.position = adjustedPosition;

            ulong objectId = eventInstance.GetComponent<NetworkObject>().NetworkObjectId;
            if (eventInfoDictionary.ContainsKey(i))
            {
                EventInfoData eventData = eventInfoDictionary[i];
                Debug.Log("Cargando información del diccionario en cliente.");
                UpdateEventInfoPanel(eventInstance, eventData.title, eventData.info, eventData.date, eventData.time);
            }
        }
    }

    private void PrintEventInfoDictionary()
    {
        Debug.Log("Imprimiendo información del diccionario:");
        Debug.Log($"Número de objetos en el diccionario: {eventInfoDictionary.Count}");
        foreach (var kvp in eventInfoDictionary)
        {
            int index = kvp.Key;
            EventInfoData eventData = kvp.Value;
            Debug.Log($"Índice: {index}, Título: {eventData.title}, Información: {eventData.info}, Fecha: {eventData.date}, Hora: {eventData.time}");
        }
    }

    //Actualizar a coordenada de mapa al desplazarse
    void UpdateEventPosition(GameObject eventInstance, Vector2d geoPosition)
    {
        // Convierte la posición geográfica a una posición en el mundo utilizando el mapa
        Vector3 worldPosition = map.GeoToWorldPosition(geoPosition, true);

        // Asegura que la altura (Y) sea constante
        worldPosition.y = 5;

        // Actualiza la posición del evento en el mundo
        eventInstance.transform.position = worldPosition;
    }

    #endregion
}