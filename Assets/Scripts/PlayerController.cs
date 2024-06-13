using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Mapbox.Unity.Location;
using Mapbox.Utils;

public class PlayerController : NetworkBehaviour
{
    private ILocationProvider _locationProvider;
    private NetworkVariable<Vector3> syncedPosition = new NetworkVariable<Vector3>();

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        if (IsLocalPlayer)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        CheckSceneAndActivate();
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneAndActivate();
    }

    private void CheckSceneAndActivate()
    {
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
