using Unity.Netcode;
using Mapbox.Utils;

public struct NetworkVector2d : INetworkSerializable
{
    public Vector2d Value;

    public NetworkVector2d(Vector2d value)
    {
        Value = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            serializer.GetFastBufferReader().ReadValueSafe(out Value);
        }
        else
        {
            serializer.GetFastBufferWriter().WriteValueSafe(Value);
        }
    }
}
