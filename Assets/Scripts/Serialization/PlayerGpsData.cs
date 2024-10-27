using Mapbox.Utils;
using Unity.Netcode;
using UnityEngine;

public struct PlayerGpsData : INetworkSerializable
{
    public ulong ClientId;
    public Vector2 GpsPosition; // Vector2 es serializable por Netcode

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref GpsPosition);
    }

    public PlayerGpsData(ulong clientId, Vector2 gpsPosition)
    {
        ClientId = clientId;
        GpsPosition = gpsPosition;
    }
}
