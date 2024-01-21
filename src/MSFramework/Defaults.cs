using System;
using MicroserviceFramework.Serialization;
using MongoDB.Bson;

namespace MicroserviceFramework;

public static class Defaults
{
    public static IJsonSerializer JsonSerializer;

    public static class Headers
    {
        public const string InternalCall = "Internal-Call";
    }

    public static class Types
    {
        public static readonly Type String = typeof(string);
        public static readonly Type Guid = typeof(Guid);
        public static readonly Type ObjectId = typeof(ObjectId);
    }
}
