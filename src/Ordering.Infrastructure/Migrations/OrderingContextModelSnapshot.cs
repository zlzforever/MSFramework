﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Ordering.Infrastructure;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    [DbContext(typeof(OrderingContext))]
    partial class OrderingContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("DatabaseType", "mysql")
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("MicroserviceFramework.Auditing.Model.AuditEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<string>("EntityId")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("entity_id");

                    b.Property<string>("OperationId")
                        .HasColumnType("varchar(36)")
                        .HasColumnName("operation_id");

                    b.Property<string>("OperationType")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("operation_type");

                    b.Property<string>("Type")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
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
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<long?>("CreationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("creator_id");

                    b.Property<string>("CreatorName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("creator_name");

                    b.Property<string>("DeviceId")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("device_id");

                    b.Property<string>("DeviceModel")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("device_model");

                    b.Property<int>("Elapsed")
                        .HasColumnType("int")
                        .HasColumnName("elapsed");

                    b.Property<long>("EndTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasDefaultValue(0L)
                        .HasColumnName("end_time");

                    b.Property<string>("IP")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("ip");

                    b.Property<double?>("Lat")
                        .HasColumnType("double")
                        .HasColumnName("lat");

                    b.Property<double?>("Lng")
                        .HasColumnType("double")
                        .HasColumnName("lng");

                    b.Property<string>("TraceId")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)")
                        .HasColumnName("trace_id");

                    b.Property<string>("Url")
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)")
                        .HasColumnName("url");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)")
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
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<string>("EntityId")
                        .HasColumnType("varchar(36)")
                        .HasColumnName("entity_id");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("name");

                    b.Property<string>("NewValue")
                        .HasColumnType("longtext")
                        .HasColumnName("new_value");

                    b.Property<string>("OriginalValue")
                        .HasColumnType("longtext")
                        .HasColumnName("original_value");

                    b.Property<string>("Type")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.ToTable("ordering_audit_property");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<string>("BuyerId2")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("buyer_id2");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("concurrency_stamp");

                    b.Property<long?>("CreationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("creator_id");

                    b.Property<string>("CreatorName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("creator_name");

                    b.Property<string>("Description2")
                        .HasMaxLength(2000)
                        .HasColumnType("varchar(2000)")
                        .HasColumnName("description2");

                    b.Property<string>("DictJson")
                        .HasColumnType("JSON")
                        .HasColumnName("dict_json");

                    b.Property<string>("Extras")
                        .HasColumnType("JSON")
                        .HasColumnName("extras");

                    b.Property<string>("ListJson")
                        .HasColumnType("JSON")
                        .HasColumnName("list_json");

                    b.Property<int?>("OperatorId")
                        .HasColumnType("int")
                        .HasColumnName("operator_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)")
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
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<decimal>("Discount")
                        .HasColumnType("decimal(65,30)")
                        .HasColumnName("discount");

                    b.Property<string>("OrderId")
                        .HasColumnType("varchar(36)")
                        .HasColumnName("order_id");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(65,30)")
                        .HasColumnName("unit_price");

                    b.Property<int>("Units")
                        .HasColumnType("int")
                        .HasColumnName("units");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("ordering_order_item");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.Product", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("concurrency_stamp");

                    b.Property<long?>("CreationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("creator_id");

                    b.Property<string>("CreatorName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("creator_name");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("name");

                    b.Property<int>("Price")
                        .HasColumnType("int")
                        .HasColumnName("price");

                    b.HasKey("Id");

                    b.ToTable("ordering_product");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
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
                                .HasColumnType("varchar(36)")
                                .HasColumnName("id");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("varchar(200)")
                                .HasColumnName("address_city");

                            b1.Property<string>("Country")
                                .IsRequired()
                                .HasMaxLength(50)
                                .HasColumnType("varchar(50)")
                                .HasColumnName("address_country");

                            b1.Property<string>("State")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("varchar(200)")
                                .HasColumnName("address_state");

                            b1.Property<string>("Street")
                                .IsRequired()
                                .HasMaxLength(200)
                                .HasColumnType("varchar(200)")
                                .HasColumnName("address_street");

                            b1.Property<string>("ZipCode")
                                .IsRequired()
                                .HasMaxLength(20)
                                .HasColumnType("varchar(20)")
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
                                .HasColumnType("varchar(36)")
                                .HasColumnName("id");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasMaxLength(255)
                                .HasColumnType("varchar(255)")
                                .HasColumnName("product_name");

                            b1.Property<string>("PictureUrl")
                                .HasMaxLength(300)
                                .HasColumnType("varchar(300)")
                                .HasColumnName("product_picture_url");

                            b1.Property<string>("ProductId")
                                .IsRequired()
                                .HasMaxLength(36)
                                .HasColumnType("varchar(36)")
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
