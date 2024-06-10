using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RealTimeEventSpawner : NetworkBehaviour
{
    [SerializeField]
    GameObject eventPrefab; // Prefab del evento a instanciar

    public List<GameObject> spawnedEvents = new List<GameObject>();
    public List<Vector3> eventPositions = new List<Vector3>(); // Cambiado a Vector3

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
        // Recrear los pines en el Start
        RecreateEvents();
    }

    // M�todo llamado por los clientes para solicitar la creaci�n de un evento
    [ServerRpc(RequireOwnership = false)]
    public void CreateEventServerRpc(Vector3 worldPosition)
    {
        if (IsServer)
        {
            // Comprobar si la ubicaci�n ya est� en la lista de eventos
            if (!eventPositions.Contains(worldPosition))
            {
                // A�adir la ubicaci�n del evento a la lista
                eventPositions.Add(worldPosition);
                // Notificar a todos los clientes para crear el evento en sus mapas
                CreateEventClientRpc(worldPosition);
            }
        }
    }

    // M�todo que crea el evento en el servidor
    void CreateEvent(Vector3 worldPosition)
    {
        Debug.Log("Creating event");
        worldPosition.y = 5; // Establecer la Y en 5
        GameObject eventInstance = Instantiate(eventPrefab, worldPosition, Quaternion.identity);

        NetworkObject networkObject = eventInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }

        spawnedEvents.Add(eventInstance);

        // Asegurarse de que la posici�n Y no cambie despu�s de la instanciaci�n
        eventInstance.transform.position = new Vector3(worldPosition.x, 5, worldPosition.z);
    }

    // M�todo llamado por el servidor para notificar a todos los clientes de la creaci�n del evento
    [ClientRpc]
    void CreateEventClientRpc(Vector3 worldPosition)
    {
        CreateEvent(worldPosition);
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

    // M�todo para manejar la carga de escenas
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Location-basedGame") // Aseg�rate de que este es el nombre correcto de tu escena
        {
            Debug.Log("VOLVIMOS ESCENA LOCATION");
            // Activar el spawner y recrear los eventos al cargar la escena original
            gameObject.SetActive(true);

            //instancia de nuevo los pines
            RecreateEvents();
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
            Debug.Log(worldPosition.ToString());
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

            eventInstance.transform.position = adjustedPosition;
        }
    }
}
