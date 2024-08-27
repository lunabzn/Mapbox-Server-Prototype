using Mapbox.Unity.Location;
using Mapbox.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    public GameObject alert;
    public static NetworkManagerUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUnityServices();
    }

    private async void InitializeUnityServices()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        CreateRelayAndStartServer();
#elif UNITY_ANDROID
        JoinRelayAndConnect();
#endif
    }

    private async void CreateRelayAndStartServer()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(20);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.StartServer();

            var lobby = await LobbyService.Instance.CreateLobbyAsync("LobbyName", 4, new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject> { { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
            });

            Debug.Log($"Servidor iniciado con código de unión: {joinCode}");

            // Iniciar el heartbeat para mantener el lobby activo
            StartLobbyHeartbeat(lobby.Id);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void JoinRelayAndConnect()
    {
        try
        {
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                return;
            }

            var lobbies = await LobbyService.Instance.QueryLobbiesAsync();
            if (lobbies.Results.Count > 0)
            {
                var joinCode = lobbies.Results[0].Data["joinCode"].Value;

                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

                NetworkManager.Singleton.StartClient();
            }
            else
            {
                Debug.Log("No se encontraron lobbies disponibles.");
                alert.SetActive(true);
            }
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void StartLobbyHeartbeat(string lobbyId)
    {
        while (true)
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
                Debug.Log("Heartbeat enviado para el lobby " + lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Error enviando heartbeat: " + e);
                break; // Salir del bucle si no se puede enviar el heartbeat
            }

            await Task.Delay(10000); // Envía un heartbeat cada 10 segundos
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Cliente conectado con ID: {clientId}");

        if (NetworkManager.Singleton.IsServer)
        {
            // Lógica para manejar la conexión del cliente y spawnear el jugador
            Vector2d latLon = LocationProviderFactory.Instance.DefaultLocationProvider.CurrentLocation.LatitudeLongitude;
            Vector3 spawnPosition = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(latLon);

            // Instanciar jugador para el cliente conectado
            var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }

    private void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
