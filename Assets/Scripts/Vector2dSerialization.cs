using Unity.Netcode;
using Mapbox.Utils;

public static class Vector2dSerialization
{
    public static void WriteValueSafe(this FastBufferWriter writer, in Vector2d value)
    {
        writer.WriteValueSafe(value.x);
        writer.WriteValueSafe(value.y);
    }

    public static void ReadValueSafe(this FastBufferReader reader, out Vector2d value)
    {
        reader.ReadValueSafe(out double x);
        reader.ReadValueSafe(out double y);
        value = new Vector2d(x, y);
    }
}
