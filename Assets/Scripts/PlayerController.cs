using UnityEngine;
using Unity.Netcode;
using Mapbox.Unity.Location;
using UnityEngine.SceneManagement;
using Mapbox.Utils;

public class PlayerController : NetworkBehaviour
{
    private ILocationProvider _locationProvider;
    private NetworkVariable<Vector3> syncedPosition = new NetworkVariable<Vector3>();

    private static PlayerController instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (IsLocalPlayer)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Location-basedGame")
        {
            if (IsLocalPlayer)
            {
                gameObject.SetActive(true);

                if (!IsPlayerInstanceExists())
                {
                    Vector2d latLon = _locationProvider.CurrentLocation.LatitudeLongitude;
                    RequestSpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, latLon);
                }
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (IsLocalPlayer)
        {
            Vector3 currentPosition = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(_locationProvider.CurrentLocation.LatitudeLongitude);
            transform.localPosition = currentPosition;
            UpdatePositionServerRpc(currentPosition);
        }
        else
        {
            transform.localPosition = syncedPosition.Value;
        }
    }

    [ServerRpc]
    void UpdatePositionServerRpc(Vector3 position)
    {
        syncedPosition.Value = position;
    }

    [ServerRpc]
    void RequestSpawnPlayerServerRpc(ulong clientId, Vector2d latLon)
    {
        if (IsPlayerInstanceExists()) return;

        Vector3 spawnPosition = LocationProviderFactory.Instance.mapManager.GeoToWorldPosition(latLon);
        var playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
        var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    bool IsPlayerInstanceExists()
    {
        // Check if there is already a player instance for this client
        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            if (player.IsLocalPlayer)
            {
                return true;
            }
        }
        return false;
    }
}
