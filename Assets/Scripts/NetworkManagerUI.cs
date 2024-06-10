using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    public GameObject alert;
    public static NetworkManagerUI Instance { get; private set; }

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await UnityServices.InitializeAsync();

        // Autenticar al jugador
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
            var allocation = await RelayService.Instance.CreateAllocationAsync(4); // Hasta 4 jugadores
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData // For the host, the connection data and host connection data are the same
            );

            NetworkManager.Singleton.StartServer();

            // Crea un lobby y almacena el join code
            var lobby = await LobbyService.Instance.CreateLobbyAsync("LobbyName", 4, new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject> {
                { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
            }
            });

            Debug.Log($"Servidor iniciado con código de unión: {joinCode}");
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
            // Verificar si ya estamos conectados y evitar intentar conectarse nuevamente
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                return;
            }

            // Buscar un lobby disponible
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
    private void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }


}
