using System;
using MongoDB.Bson;

namespace MicroserviceFramework;

public static class Consts
{
    public static class Types
    {
        public static readonly Type String = typeof(string);
        public static readonly Type Guid = typeof(Guid);
        public static readonly Type ObjectId = typeof(ObjectId);
    }
}
