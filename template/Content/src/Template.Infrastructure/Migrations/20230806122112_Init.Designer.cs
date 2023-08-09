﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Template.Infrastructure;

#nullable disable

namespace Template.Infrastructure.Migrations
{
    [DbContext(typeof(TemplateDbContext))]
    [Migration("20230806122112_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("MicroserviceFramework.Auditing.AuditEntity", b =>
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

                    b.ToTable("audit_entity");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.AuditOperation", b =>
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
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("creator_name");

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

                    b.HasIndex("CreatorId");

                    b.HasIndex("EndTime");

                    b.ToTable("audit_operation");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.AuditProperty", b =>
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

                    b.ToTable("audit_property");
                });

            modelBuilder.Entity("Template.Domain.Aggregates.Project.Product", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(36)
                        .HasColumnType("varchar")
                        .HasColumnName("id");

                    b.Property<long?>("CreationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("creation_time");

                    b.Property<string>("CreatorId")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("creator_id");

                    b.Property<string>("CreatorName")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("creator_name");

                    b.Property<long?>("LastModificationTime")
                        .HasColumnType("bigint")
                        .HasColumnName("last_modification_time");

                    b.Property<string>("LastModifierId")
                        .HasMaxLength(36)
                        .HasColumnType("varchar(36)")
                        .HasColumnName("last_modifier_id");

                    b.Property<string>("LastModifierName")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("last_modifier_name");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("name");

                    b.Property<int>("Price")
                        .HasColumnType("int")
                        .HasColumnName("price");

                    b.Property<string>("ProductType")
                        .HasColumnType("longtext")
                        .HasColumnName("product_type");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("product");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.AuditEntity", b =>
                {
                    b.HasOne("MicroserviceFramework.Auditing.AuditOperation", "Operation")
                        .WithMany("Entities")
                        .HasForeignKey("OperationId");

                    b.Navigation("Operation");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.AuditProperty", b =>
                {
                    b.HasOne("MicroserviceFramework.Auditing.AuditEntity", "Entity")
                        .WithMany("Properties")
                        .HasForeignKey("EntityId");

                    b.Navigation("Entity");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.AuditEntity", b =>
                {
                    b.Navigation("Properties");
                });

            modelBuilder.Entity("MicroserviceFramework.Auditing.AuditOperation", b =>
                {
                    b.Navigation("Entities");
                });
#pragma warning restore 612, 618
        }
    }
}