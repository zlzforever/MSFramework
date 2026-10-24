using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MicroserviceFramework.Ef.Internal;

internal static class CanonicalJson
{
    public static bool Equals<T>(T left, T right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return Serialize(left).AsSpan().SequenceEqual(Serialize(right));
    }

    public static int GetHashCode<T>(T instance)
    {
        if (instance is null)
        {
            return 0;
        }

        var bytes = Serialize(instance);
        unchecked
        {
            var hash = (int)2166136261;
            foreach (var value in bytes)
            {
                hash = (hash ^ value) * 16777619;
            }

            return hash;
        }
    }

    private static byte[] Serialize<T>(T instance)
    {
        var json = Defaults.JsonSerializer.Serialize(instance);
        using var document = JsonDocument.Parse(json);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteElement(writer, document.RootElement);
        }

        return stream.ToArray();
    }

    private static void WriteElement(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteElement(writer, property.Value);
                }

                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteElement(writer, item);
                }

                writer.WriteEndArray();
                break;
            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;
            case JsonValueKind.Number:
                writer.WriteRawValue(element.GetRawText(), true);
                break;
            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;
            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                writer.WriteNullValue();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(element), element.ValueKind,
                    "Unsupported JSON value kind.");
        }
    }
}
