﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MicroserviceFramework.Extensions;

namespace MicroserviceFramework.Text.Json.Converters;

/// <summary>
///
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TryGetInt32(out var v) ? DateTimeOffset.FromUnixTimeSeconds(v).LocalDateTime : DateTime.UnixEpoch;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeSeconds());
    }
}
