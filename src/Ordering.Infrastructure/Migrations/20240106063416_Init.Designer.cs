﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Ordering.Infrastructure;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    [DbContext(typeof(OrderingContext))]
    [Migration("20240106063416_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MicroserviceFramework.Auditing.Model.AuditEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("id");

                    b.Property<string>("EntityId")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("entity_id");

                    b.Property<string>("OperationId")
                        .HasColumnType("character varying(36)")
                        .HasColumnName("operation_id");

                    b.Property<string>("OperationType")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("operation_type");

                    b.Property<string>("Type")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.HasIndex("OperationId");

                    b.ToTable("ordering_audit_entity");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.Model.AuditOperation", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("id");

                    b.Property<long?>("CreationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("creator_id");

                    b.Property<string>("CreatorName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("creator_name");

                    b.Property<string>("DeviceId")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("device_id");

                    b.Property<string>("DeviceModel")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("device_model");

                    b.Property<int>("Elapsed")
                        .HasColumnType("integer")
                        .HasColumnName("elapsed");

                    b.Property<long>("EndTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValue(0L)
                        .HasColumnName("end_time");

                    b.Property<string>("IP")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("ip");

                    b.Property<double?>("Lat")
                        .HasColumnType("double precision")
                        .HasColumnName("lat");

                    b.Property<double?>("Lng")
                        .HasColumnType("double precision")
                        .HasColumnName("lng");

                    b.Property<string>("TraceId")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("trace_id");

                    b.Property<string>("Url")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasColumnName("url");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)")
                        .HasColumnName("user_agent");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.HasIndex("EndTime");

                    b.ToTable("ordering_audit_operation");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.Model.AuditProperty", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("id");

                    b.Property<string>("EntityId")
                        .HasColumnType("character varying(36)")
                        .HasColumnName("entity_id");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("name");

                    b.Property<string>("NewValue")
                        .HasColumnType("text")
                        .HasColumnName("new_value");

                    b.Property<string>("OriginalValue")
                        .HasColumnType("text")
                        .HasColumnName("original_value");

                    b.Property<string>("Type")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.ToTable("ordering_audit_property");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("id");

                    b.Property<string>("BuyerId2")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("buyer_id2");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("concurrency_stamp");

                    b.Property<long?>("CreationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("creator_id");

                    b.Property<string>("CreatorName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("creator_name");

                    b.Property<string>("Description2")
                        .HasMaxLength(2000)
                        .HasColumnType("character varying(2000)")
                        .HasColumnName("description2");

                    b.Property<string>("DictJson")
                        .HasColumnType("JSONB")
                        .HasColumnName("dict_json");

                    b.Property<string>("Extras")
                        .HasColumnType("JSONB")
                        .HasColumnName("extras");

                    b.Property<string>("ListJson")
                        .HasColumnType("JSONB")
                        .HasColumnName("list_json");

                    b.Property<int?>("OperatorId")
                        .HasColumnType("integer")
                        .HasColumnName("operator_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex("CreationTime");

                    b.HasIndex("OperatorId");

                    b.ToTable("ordering_order");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.OrderItem", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("id");

                    b.Property<decimal>("Discount")
                        .HasColumnType("numeric")
                        .HasColumnName("discount");

                    b.Property<string>("OrderId")
                        .HasColumnType("character varying(36)")
                        .HasColumnName("order_id");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("numeric")
                        .HasColumnName("unit_price");

                    b.Property<int>("Units")
                        .HasColumnType("integer")
                        .HasColumnName("units");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("ordering_order_item");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.Product", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("concurrency_stamp");

                    b.Property<long?>("CreationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(36)
                        .HasColumnType("character varying(36)")
                        .HasColumnName("creator_id");

                    b.Property<string>("CreatorName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("creator_name");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("name");

                    b.Property<int>("Price")
                        .HasColumnType("integer")
                        .HasColumnName("price");

                    b.HasKey("Id");

                    b.ToTable("ordering_product");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("user", (string)null);
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.Model.AuditEntity", b =>
                {
                    b.HasOne("MicroserviceFramework.Auditing.Model.AuditOperation", "Operation")
                        .WithMany("Entities")
                        .HasForeignKey("OperationId");

                    b.Navigation("Operation");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.Model.AuditProperty", b =>
                {
                    b.HasOne("MicroserviceFramework.Auditing.Model.AuditEntity", "Entity")
                        .WithMany("Properties")
                        .HasForeignKey("EntityId");

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.Order", b =>
                {
                    b.HasOne("Ordering.Domain.AggregateRoots.User", "Operator")
                        .WithMany()
                        .HasForeignKey("OperatorId");

                    b.OwnsOne("Ordering.Domain.AggregateRoots.Address", "Address", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("character varying(36)")
                                .HasColumnName("id");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("character varying(200)")
                                .HasColumnName("address_city");

                            b1.Property<string>("Country")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("character varying(50)")
                                .HasColumnName("address_country");

                            b1.Property<string>("State")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("character varying(200)")
                                .HasColumnName("address_state");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("character varying(200)")
                                .HasColumnName("address_street");

                            b1.Property<string>("ZipCode")
                                .IsRequired()
                                .HasMaxLength(20)
                                .HasColumnType("character varying(20)")
                                .HasColumnName("address_zip_code");

                            b1.HasKey("OrderId");

                            b1.ToTable("ordering_order");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");
                        });

                    b.Navigation("Address");

                    b.Navigation("Operator");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.OrderItem", b =>
                {
                    b.HasOne("Ordering.Domain.AggregateRoots.Order", "Order")
                        .WithMany("Items")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.ClientCascade);

                    b.OwnsOne("Ordering.Domain.AggregateRoots.OrderProduct", "Product", b1 =>
                        {
                            b1.Property<string>("OrderItemId")
                                .HasColumnType("character varying(36)")
                                .HasColumnName("id");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("character varying(255)")
                                .HasColumnName("product_name");

                            b1.Property<string>("PictureUrl")
                                .HasMaxLength(300)
                                .HasColumnType("character varying(300)")
                                .HasColumnName("product_picture_url");

                            b1.Property<string>("ProductId")
                                .IsRequired()
                                .HasMaxLength(36)
                                .HasColumnType("character varying(36)")
                                .HasColumnName("product_id");

                            b1.HasKey("OrderItemId");

                            b1.HasIndex("ProductId");

                            b1.ToTable("ordering_order_item");

                            b1.WithOwner()
                                .HasForeignKey("OrderItemId");
                        });

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.Model.AuditEntity", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.Model.AuditOperation", b =>
                {
                    b.Navigation("Entities");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.Order", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
