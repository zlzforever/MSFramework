﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MicroserviceFramework.Audit.AuditEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<string>("EntityId")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("entity_id");

                    b.Property<string>("OperationId")
                        .HasColumnType("varchar(36)")
                        .HasColumnName("operation_id");

                    b.Property<string>("OperationType")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("operation_type");

                    b.Property<string>("Type")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.HasIndex("OperationId");

                    b.ToTable("audit_entity", (string)null);
                });

            modelBuilder.Entity("MicroserviceFramework.Audit.AuditOperation", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<long?>("CreationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("creator_id");

                    b.Property<string>("DeviceId")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("device_id");

                    b.Property<string>("DeviceModel")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
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
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("ip");

                    b.Property<double?>("Lat")
                        .HasColumnType("double")
                        .HasColumnName("lat");

                    b.Property<double?>("Lng")
                        .HasColumnType("double")
                        .HasColumnName("lng");

                    b.Property<string>("Url")
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)")
                        .HasColumnName("url");

                    b.Property<string>("UserAgent")
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)")
                        .HasColumnName("user_agent");

                    b.HasKey("Id");

                    b.HasIndex("CreationTime");

                    b.HasIndex("CreatorId");

                    b.HasIndex("EndTime");

                    b.ToTable("audit_operation", (string)null);
                });

            modelBuilder.Entity("MicroserviceFramework.Audit.AuditProperty", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<string>("EntityId")
                        .HasColumnType("varchar(36)")
                        .HasColumnName("entity_id");

                    b.Property<string>("Name")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("name");

                    b.Property<string>("NewValue")
                        .HasColumnType("longtext")
                        .HasColumnName("new_value");

                    b.Property<string>("OriginalValue")
                        .HasColumnType("longtext")
                        .HasColumnName("original_value");

                    b.Property<string>("Type")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.HasIndex("EntityId");

                    b.ToTable("audit_property", (string)null);
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("id");

                    b.Property<string>("BuyerId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("buyer_id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("concurrency_stamp");

                    b.Property<DateTimeOffset?>("CreationTime")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasColumnType("longtext")
                        .HasColumnName("creator_id");

                    b.Property<string>("Description")
                        .HasColumnType("longtext")
                        .HasColumnName("description");

                    b.Property<string>("Dict")
                        .HasColumnType("JSON")
                        .HasColumnName("dict");

                    b.Property<string>("RivalNetworks")
                        .HasColumnType("JSON")
                        .HasColumnName("rival_networks");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.ToTable("order");
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

                    b.Property<string>("PictureUrl")
                        .HasColumnType("longtext")
                        .HasColumnName("picture_url");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("char(36)")
                        .HasColumnName("product_id");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("product_name");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(65,30)")
                        .HasColumnName("unit_price");

                    b.Property<int>("Units")
                        .HasColumnType("int")
                        .HasColumnName("units");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("order_item");
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

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)")
                        .HasColumnName("name");

                    b.Property<int>("Price")
                        .HasColumnType("int")
                        .HasColumnName("price");

                    b.HasKey("Id");

                    b.ToTable("product");
                });

            modelBuilder.Entity("MicroserviceFramework.Audit.AuditEntity", b =>
                {
                    b.HasOne("MicroserviceFramework.Audit.AuditOperation", "Operation")
                        .WithMany("Entities")
                        .HasForeignKey("OperationId");

                    b.Navigation("Operation");
                });

            modelBuilder.Entity("MicroserviceFramework.Audit.AuditProperty", b =>
                {
                    b.HasOne("MicroserviceFramework.Audit.AuditEntity", "Entity")
                        .WithMany("Properties")
                        .HasForeignKey("EntityId");

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.Order", b =>
                {
                    b.OwnsOne("Ordering.Domain.AggregateRoots.Address", "Address", b1 =>
                        {
                            b1.Property<string>("OrderId")
                                .HasColumnType("varchar(36)")
                                .HasColumnName("id");

                            b1.Property<string>("City")
                                .IsRequired()
                                .HasColumnType("longtext")
                                .HasColumnName("address_city");

                            b1.Property<string>("Country")
                                .HasColumnType("longtext")
                                .HasColumnName("address_country");

                            b1.Property<string>("State")
                                .HasColumnType("longtext")
                                .HasColumnName("address_state");

                            b1.Property<string>("Street")
                                .HasColumnType("longtext")
                                .HasColumnName("address_street");

                            b1.Property<string>("ZipCode")
                                .HasColumnType("longtext")
                                .HasColumnName("address_zip_code");

                            b1.HasKey("OrderId");

                            b1.ToTable("order");

                            b1.WithOwner()
                                .HasForeignKey("OrderId");
                        });

                    b.Navigation("Address");
                });

            modelBuilder.Entity("Ordering.Domain.AggregateRoots.OrderItem", b =>
                {
                    b.HasOne("Ordering.Domain.AggregateRoots.Order", null)
                        .WithMany("Items")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.ClientCascade);
                });

            modelBuilder.Entity("MicroserviceFramework.Audit.AuditEntity", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("MicroserviceFramework.Audit.AuditOperation", b =>
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
