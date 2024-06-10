using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RealTimeEventSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject eventPrefab; // Prefab del evento a instanciar

    public List<GameObject> spawnedEvents = new List<GameObject>();
    public List<Vector3> eventPositions = new List<Vector3>();

    public static RealTimeEventSpawner Instance { get; private set; }

    private void Awake()
    {
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

    // Método llamado por los clientes para solicitar la creación de un evento
    public void RequestCreateEvent(Vector3 worldPosition)
    {
        if (IsClient)
        {
            // El cliente solicita la creación del evento al servidor
            CreateEventServerRpc(worldPosition);
        }
    }

    // ServerRpc para manejar la solicitud de creación de evento desde el cliente
    [ServerRpc(RequireOwnership = false)]
    public void CreateEventServerRpc(Vector3 worldPosition, ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            // Comprobar si la ubicación ya está en la lista de eventos
            if (!eventPositions.Contains(worldPosition))
            {
                // Añadir la ubicación del evento a la lista
                eventPositions.Add(worldPosition);
                // Crear el evento en el servidor
                CreateEvent(worldPosition);
                // Notificar a todos los clientes para crear el evento en sus mapas
                CreateEventClientRpc(worldPosition);
            }
        }
    }

    // Método que crea el evento en el servidor
    void CreateEvent(Vector3 worldPosition)
    {
        Debug.Log("Creating event at position: " + worldPosition);
        worldPosition.y = 5; // Establecer la Y en 5
        GameObject eventInstance = Instantiate(eventPrefab, worldPosition, Quaternion.identity);

        NetworkObject networkObject = eventInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }

        spawnedEvents.Add(eventInstance);

        // Asegurarse de que la posición Y no cambie después de la instanciación
        eventInstance.transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);
    }

    // Método llamado por el servidor para notificar a todos los clientes de la creación del evento
    [ClientRpc]
    void CreateEventClientRpc(Vector3 worldPosition)
    {
        if (!IsServer)
        {
            // Solo los clientes ejecutan este código
            CreateClientEvent(worldPosition);
        }
    }

    // Método que crea el evento en el cliente (sin NetworkObject)
    void CreateClientEvent(Vector3 worldPosition)
    {
        Debug.Log("Creating client-side event at position: " + worldPosition);
        worldPosition.y = 5; // Establecer la Y en 5
        GameObject eventInstance = Instantiate(eventPrefab, worldPosition, Quaternion.identity);
        spawnedEvents.Add(eventInstance);
        eventInstance.transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);
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

    // Método para manejar la carga de escenas
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Location-basedGame") // Asegúrate de que este es el nombre correcto de tu escena
        {
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

        foreach (var worldPosition in eventPositions)
        {
            Debug.Log("Recreating event at position: " + worldPosition);
            // Establecer la Y en 5
            Vector3 adjustedPosition = new Vector3(worldPosition.x, 5, worldPosition.z);

            // Instanciar el evento en la posición calculada
            GameObject eventInstance = Instantiate(eventPrefab, adjustedPosition, Quaternion.identity);
            spawnedEvents.Add(eventInstance);

            NetworkObject networkObject = eventInstance.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Spawn();
            }

            eventInstance.transform.position = adjustedPosition;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestEventPositionsServerRpc(ServerRpcParams rpcParams = default)
    {
        // Enviar las posiciones de los eventos al cliente que lo solicitó
        SyncEventPositionsClientRpc(eventPositions.ToArray(), rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void SyncEventPositionsClientRpc(Vector3[] positions, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            eventPositions.Clear();
            eventPositions.AddRange(positions);
            RecreateClientEvents();
        }
    }

    private void RecreateClientEvents()
    {
        // Limpiar la lista de objetos instanciados
       /* foreach (var spawnedEvent in spawnedEvents)
        {
            if (spawnedEvent != null)
            {
                Destroy(spawnedEvent);
            }
        }
        spawnedEvents.Clear();
       */
        if (eventPositions.Count == 0)
        {
            Debug.Log("No hay eventos para recrear.");
            return;
        }

        foreach (var worldPosition in eventPositions)
        {
            Debug.Log("Recreating client-side event at position: " + worldPosition);
            // Establecer la Y en 5
            Vector3 adjustedPosition = new Vector3(worldPosition.x, 5, worldPosition.z);

            // Instanciar el evento en la posición calculada
            GameObject eventInstance = Instantiate(eventPrefab, adjustedPosition, Quaternion.identity);
            spawnedEvents.Add(eventInstance);
            eventInstance.transform.position = adjustedPosition;
        }
    }
}
