using System;
using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization;
using MongoDB.Bson;

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
public static class Defaults
{
    /// <summary>
    ///
    /// </summary>
    public static IJsonSerializer JsonSerializer;

    /// <summary>
    ///
    /// </summary>
    public static readonly bool IsInTests;

    static Defaults()
    {
        string[] names = ["ReSharperTestRunner", "testhost"];
        IsInTests = names.Contains(Assembly.GetEntryAssembly()?.GetName().Name);
    }

    /// <summary>
    ///
    /// </summary>
    public static class Headers
    {
        /// <summary>
        ///
        /// </summary>
        public const string InternalCall = "Internal-Call";
    }

    /// <summary>
    ///
    /// </summary>
    public static class Types
    {
        /// <summary>
        ///
        /// </summary>
        public static readonly Type String = typeof(string);

        /// <summary>
        ///
        /// </summary>
        public static readonly Type Guid = typeof(Guid);

        /// <summary>
        ///
        /// </summary>
        public static readonly Type ObjectId = typeof(ObjectId);

        /// <summary>
        ///
        /// </summary>
        public static readonly Type OptimisticLock = typeof(IOptimisticLock);

        /// <summary>
        ///
        /// </summary>
        public static readonly Type Entity = typeof(IEntity);

        /// <summary>
        ///
        /// </summary>
        public static readonly Type Repository = typeof(IRepository);

        /// <summary>
        ///
        /// </summary>
        public static readonly Type ValueObject = typeof(ValueObject);
    }
}
