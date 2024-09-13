﻿// <auto-generated />
using System;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Ordering.Domain.AggregateRoots;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;

#pragma warning disable 219, 612, 618
#nullable disable

namespace Ordering.Infrastructure.CompileModels
{
    internal partial class ProductEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "Ordering.Domain.AggregateRoots.Product",
                typeof(Product),
                baseEntityType);

            var id = runtimeEntityType.AddProperty(
                "Id",
                typeof(string),
                propertyInfo: typeof(EntityBase<string>).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(EntityBase<string>).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw,
                maxLength: 36);
            id.TypeMapping = MySqlStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                keyComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "varchar(36)",
                    size: 36));
            id.AddAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.None);
            id.AddAnnotation("Relational:ColumnName", "id");

            var concurrencyStamp = runtimeEntityType.AddProperty(
                "ConcurrencyStamp",
                typeof(string),
                propertyInfo: typeof(Product).GetProperty("ConcurrencyStamp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Product).GetField("<ConcurrencyStamp>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                concurrencyToken: true,
                maxLength: 36);
            concurrencyStamp.TypeMapping = MySqlStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                keyComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "varchar(36)",
                    size: 36));
            concurrencyStamp.AddAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.None);
            concurrencyStamp.AddAnnotation("Relational:ColumnName", "concurrency_stamp");

            var creationTime = runtimeEntityType.AddProperty(
                "CreationTime",
                typeof(DateTimeOffset?),
                propertyInfo: typeof(CreationEntity<string>).GetProperty("CreationTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(CreationEntity<string>).GetField("<CreationTime>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            creationTime.TypeMapping = MySqlDateTimeOffsetTypeMapping.Default.Clone(
                comparer: new ValueComparer<DateTimeOffset?>(
                    (Nullable<DateTimeOffset> v1, Nullable<DateTimeOffset> v2) => v1.HasValue && v2.HasValue && ((DateTimeOffset)v1).EqualsExact((DateTimeOffset)v2) || !v1.HasValue && !v2.HasValue,
                    (Nullable<DateTimeOffset> v) => v.HasValue ? ((DateTimeOffset)v).GetHashCode() : 0,
                    (Nullable<DateTimeOffset> v) => v.HasValue ? (Nullable<DateTimeOffset>)(DateTimeOffset)v : default(Nullable<DateTimeOffset>)),
                keyComparer: new ValueComparer<DateTimeOffset?>(
                    (Nullable<DateTimeOffset> v1, Nullable<DateTimeOffset> v2) => v1.HasValue && v2.HasValue && ((DateTimeOffset)v1).EqualsExact((DateTimeOffset)v2) || !v1.HasValue && !v2.HasValue,
                    (Nullable<DateTimeOffset> v) => v.HasValue ? ((DateTimeOffset)v).GetHashCode() : 0,
                    (Nullable<DateTimeOffset> v) => v.HasValue ? (Nullable<DateTimeOffset>)(DateTimeOffset)v : default(Nullable<DateTimeOffset>)),
                providerValueComparer: new ValueComparer<DateTimeOffset?>(
                    (Nullable<DateTimeOffset> v1, Nullable<DateTimeOffset> v2) => v1.HasValue && v2.HasValue && ((DateTimeOffset)v1).EqualsExact((DateTimeOffset)v2) || !v1.HasValue && !v2.HasValue,
                    (Nullable<DateTimeOffset> v) => v.HasValue ? ((DateTimeOffset)v).GetHashCode() : 0,
                    (Nullable<DateTimeOffset> v) => v.HasValue ? (Nullable<DateTimeOffset>)(DateTimeOffset)v : default(Nullable<DateTimeOffset>)),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "datetime(6)",
                    precision: 6));
            creationTime.AddAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.None);
            creationTime.AddAnnotation("Relational:ColumnName", "creation_time");

            var creatorId = runtimeEntityType.AddProperty(
                "CreatorId",
                typeof(string),
                propertyInfo: typeof(CreationEntity<string>).GetProperty("CreatorId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(CreationEntity<string>).GetField("<CreatorId>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                maxLength: 36);
            creatorId.TypeMapping = MySqlStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                keyComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "varchar(36)",
                    size: 36));
            creatorId.AddAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.None);
            creatorId.AddAnnotation("Relational:ColumnName", "creator_id");

            var creatorName = runtimeEntityType.AddProperty(
                "CreatorName",
                typeof(string),
                propertyInfo: typeof(CreationEntity<string>).GetProperty("CreatorName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(CreationEntity<string>).GetField("<CreatorName>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                maxLength: 256);
            creatorName.TypeMapping = MySqlStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                keyComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "varchar(256)",
                    size: 256));
            creatorName.AddAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.None);
            creatorName.AddAnnotation("Relational:ColumnName", "creator_name");

            var name = runtimeEntityType.AddProperty(
                "Name",
                typeof(string),
                propertyInfo: typeof(Product).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Product).GetField("<Name>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                maxLength: 256);
            name.TypeMapping = MySqlStringTypeMapping.Default.Clone(
                comparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                keyComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                providerValueComparer: new ValueComparer<string>(
                    (string v1, string v2) => v1 == v2,
                    (string v) => v.GetHashCode(),
                    (string v) => v),
                mappingInfo: new RelationalTypeMappingInfo(
                    storeTypeName: "varchar(256)",
                    size: 256));
            name.AddAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.None);
            name.AddAnnotation("Relational:ColumnName", "name");

            var price = runtimeEntityType.AddProperty(
                "Price",
                typeof(int),
                propertyInfo: typeof(Product).GetProperty("Price", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(Product).GetField("<Price>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                sentinel: 0);
            price.TypeMapping = MySqlIntTypeMapping.Default.Clone(
                comparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                keyComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v),
                providerValueComparer: new ValueComparer<int>(
                    (int v1, int v2) => v1 == v2,
                    (int v) => v,
                    (int v) => v));
            price.AddAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.None);
            price.AddAnnotation("Relational:ColumnName", "price");

            var key = runtimeEntityType.AddKey(
                new[] { id });
            runtimeEntityType.SetPrimaryKey(key);

            return runtimeEntityType;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ordering_product");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
