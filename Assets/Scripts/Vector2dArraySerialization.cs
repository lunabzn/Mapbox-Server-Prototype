using Unity.Netcode;
using Mapbox.Utils;

public static class Vector2dArraySerialization
{
    public static void WriteValueSafe(this FastBufferWriter writer, in Vector2d[] value)
    {
        writer.WriteValueSafe(value.Length);
        foreach (var vec in value)
        {
            writer.WriteValueSafe(vec);
        }
    }

    public static void ReadValueSafe(this FastBufferReader reader, out Vector2d[] value)
    {
        reader.ReadValueSafe(out int length);
        value = new Vector2d[length];
        for (int i = 0; i < length; i++)
        {
            reader.ReadValueSafe(out Vector2d vec);
            value[i] = vec;
        }
    }
}
