// using System;
// using System.Linq.Expressions;
// using MicroserviceFramework.Serialization;
// using MicroserviceFramework.Text.Json;
// using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
//
// namespace MicroserviceFramework.Ef.Internal;
//
// /// <summary>
// ///
// /// </summary>
// /// <typeparam name="T"></typeparam>
// public class JsonToStringConverter<T>()
//     : ValueConverter<T, string>(ToStringValue(), ToObject())
// {
//     private static readonly IJsonSerializer JsonSerializer;
//
//     static JsonToStringConverter()
//     {
//         JsonSerializer = GetJsonSerializer();
//     }
//
//     private static Expression<Func<T, string>> ToStringValue()
//         => v =>
//             JsonSerializer.Serialize(v);
//
//     private static Expression<Func<string, T>> ToObject()
//         => v => JsonSerializer.Deserialize<T>(v);
//
//     private static IJsonSerializer GetJsonSerializer(IJsonSerializer jsonSerializer = null)
//     {
//         return jsonSerializer ??
//                (Defaults.JsonSerializer == null ? TextJsonSerializer.Create() : Defaults.JsonSerializer);
//     }
// }
