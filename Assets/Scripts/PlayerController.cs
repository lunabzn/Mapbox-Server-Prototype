using Mapbox.Unity.Location;
using Mapbox.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    // Diccionario compartido que contiene las posiciones GPS de todos los jugadores
    private static Dictionary<ulong, Vector2d> playerGpsPositions = new Dictionary<ulong, Vector2d>();

    // Proveedor de ubicación para obtener la latitud y longitud
    private ILocationProvider _locationProvider;

    //Color azul del prefab en local
    Color customBlue = new Color(0f, 0.647f, 1f, 1f);

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        // Obtener los renderers de los componentes hijo (cubo y esfera)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (IsLocalPlayer)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;

            // Cambiar el color del jugador local
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = customBlue;
            }
        }
        
        // Suscribirse al evento de cambio de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
        CheckSceneAndActivate();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneAndActivate();
    }

    private void CheckSceneAndActivate()
    {
        // Desactivar este objeto si la escena no es "Location-basedGame"
        if (SceneManager.GetActiveScene().name != "Location-basedGame")
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento cuando se destruya el objeto
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        // Si es el jugador local, actualizar su posición GPS y enviarla al servidor
        if (IsLocalPlayer)
        {
            Vector2d currentGpsPosition = _locationProvider.CurrentLocation.LatitudeLongitude;

            // Asegúrate de que las posiciones sean correctas
            //Debug.Log("Current GPS Position: " + currentGpsPosition);

            // Enviar la nueva posición GPS al servidor
            UpdateGpsPositionServerRpc(currentGpsPosition);

            // Actualizar la posición local del jugador en el mapa
            Vector3 localPosition = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(currentGpsPosition);

            //Debug.Log("World Position: " + localPosition);
            transform.position = localPosition;
        }
        else
        {
            // Para jugadores remotos, actualizar sus posiciones en el mapa local
            foreach (var playerData in playerGpsPositions)
            {
                if (playerData.Key != NetworkManager.Singleton.LocalClientId)
                {
                    Vector2d remoteGpsPosition = playerData.Value;
                    Vector3 remotePosition = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(remoteGpsPosition);
                    UpdateRemotePlayerPosition(playerData.Key, remotePosition);
                }
            }
        }
    }


    [ServerRpc]
    void UpdateGpsPositionServerRpc(Vector2d gpsPosition)
    {
        // Actualizar la posición GPS en el diccionario compartido en el servidor
        if (playerGpsPositions.ContainsKey(NetworkObject.OwnerClientId))
        {
            playerGpsPositions[NetworkObject.OwnerClientId] = gpsPosition;
        }
        else
        {
            playerGpsPositions.Add(NetworkObject.OwnerClientId, gpsPosition);
        }

        // Serializar el diccionario en un array de PlayerGpsData y enviarlo a todos los clientes
        List<PlayerGpsData> playerGpsDataList = new List<PlayerGpsData>();

        foreach (var playerData in playerGpsPositions)
        {
            playerGpsDataList.Add(new PlayerGpsData(playerData.Key, new Vector2((float)playerData.Value.x, (float)playerData.Value.y)));
        }

        UpdateAllClientsGpsPositionClientRpc(playerGpsDataList.ToArray());
    }

    [ClientRpc]
    void UpdateAllClientsGpsPositionClientRpc(PlayerGpsData[] playerGpsDataArray)
    {
        // Reconstruir el diccionario local a partir del array recibido
        playerGpsPositions.Clear();

        foreach (var playerData in playerGpsDataArray)
        {
            Vector2d gpsPosition = new Vector2d(playerData.GpsPosition.x, playerData.GpsPosition.y);
            playerGpsPositions[playerData.ClientId] = gpsPosition;
        }
    }

    // Método para actualizar la posición de los jugadores remotos en la escena local
    void UpdateRemotePlayerPosition(ulong clientId, Vector3 newPosition)
    {
        // Buscar al jugador remoto en la escena y actualizar su posición
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            if (player.OwnerClientId == clientId)
            {
                player.transform.position = newPosition;
                break;
            }
        }
    }
}
