using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "last_modifier_name",
                table: "ordering_order",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "last_modifier_id",
                table: "ordering_order",
                type: "varchar(36)",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "last_modification_time",
                table: "ordering_order",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "deletion_time",
                table: "ordering_order",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "deleter_name",
                table: "ordering_order",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "deleter_id",
                table: "ordering_order",
                type: "varchar(36)",
                maxLength: 36,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "ordering_audit_operation",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "lng",
                table: "ordering_audit_operation",
                type: "decimal(11,8)",
                precision: 11,
                scale: 8,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "lat",
                table: "ordering_audit_operation",
                type: "decimal(11,8)",
                precision: 11,
                scale: 8,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double",
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "accuracy",
                table: "ordering_audit_operation",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "altitude",
                table: "ordering_audit_operation",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "battery",
                table: "ordering_audit_operation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "bearing",
                table: "ordering_audit_operation",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "emulator",
                table: "ordering_audit_operation",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imei",
                table: "ordering_audit_operation",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "location_source",
                table: "ordering_audit_operation",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<float>(
                name: "orientation",
                table: "ordering_audit_operation",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "os_version",
                table: "ordering_audit_operation",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "platform",
                table: "ordering_audit_operation",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "query_string",
                table: "ordering_audit_operation",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "screen",
                table: "ordering_audit_operation",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "signal",
                table: "ordering_audit_operation",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "accuracy",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "altitude",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "battery",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "bearing",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "emulator",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "imei",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "location_source",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "orientation",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "os_version",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "platform",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "query_string",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "screen",
                table: "ordering_audit_operation");

            migrationBuilder.DropColumn(
                name: "signal",
                table: "ordering_audit_operation");

            migrationBuilder.AlterColumn<string>(
                name: "last_modifier_name",
                table: "ordering_order",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "last_modifier_id",
                table: "ordering_order",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldMaxLength: 36,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "last_modification_time",
                table: "ordering_order",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "deletion_time",
                table: "ordering_order",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "deleter_name",
                table: "ordering_order",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "deleter_id",
                table: "ordering_order",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(36)",
                oldMaxLength: 36,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "path",
                table: "ordering_audit_operation",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<double>(
                name: "lng",
                table: "ordering_audit_operation",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(11,8)",
                oldPrecision: 11,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "lat",
                table: "ordering_audit_operation",
                type: "double",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(11,8)",
                oldPrecision: 11,
                oldScale: 8,
                oldNullable: true);
        }
    }
}
