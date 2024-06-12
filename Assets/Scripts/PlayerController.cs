using UnityEngine;
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
}
