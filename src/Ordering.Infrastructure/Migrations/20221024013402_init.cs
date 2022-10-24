using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_operation",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    url = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    device_model = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    lat = table.Column<double>(type: "double", nullable: true),
                    lng = table.Column<double>(type: "double", nullable: true),
                    user_agent = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    end_time = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    elapsed = table.Column<int>(type: "int", nullable: false),
                    creation_time = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_operation", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dict = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    extras = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_street = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_city = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_state = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_country = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_zip_code = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rival_networks = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    buyer_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    concurrency_stamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creation_time = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    creator_id = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<int>(type: "int", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_entity",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    operation_id = table.Column<string>(type: "varchar(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    operation_type = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entity", x => x.id);
                    table.ForeignKey(
                        name: "ordering_FK_audit_entity_audit_operation_operation_id",
                        column: x => x.operation_id,
                        principalSchema: "ordering",
                        principalTable: "audit_operation",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_item",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    product_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    picture_url = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    unit_price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    discount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    units = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    order_id = table.Column<string>(type: "varchar(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.id);
                    table.ForeignKey(
                        name: "ordering_FK_order_item_order_order_id",
                        column: x => x.order_id,
                        principalSchema: "ordering",
                        principalTable: "order",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_property",
                schema: "ordering",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_id = table.Column<string>(type: "varchar(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    original_value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    new_value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_property", x => x.id);
                    table.ForeignKey(
                        name: "ordering_FK_audit_property_audit_entity_entity_id",
                        column: x => x.entity_id,
                        principalSchema: "ordering",
                        principalTable: "audit_entity",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ordering_IX_audit_entity_entity_id",
                schema: "ordering",
                table: "audit_entity",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "ordering_IX_audit_entity_operation_id",
                schema: "ordering",
                table: "audit_entity",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "ordering_IX_audit_operation_creation_time",
                schema: "ordering",
                table: "audit_operation",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "ordering_IX_audit_operation_creator_id",
                schema: "ordering",
                table: "audit_operation",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "ordering_IX_audit_operation_end_time",
                schema: "ordering",
                table: "audit_operation",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "ordering_IX_audit_property_entity_id",
                schema: "ordering",
                table: "audit_property",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "ordering_IX_order_item_order_id",
                schema: "ordering",
                table: "order_item",
                column: "order_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_property",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "order_item",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "product",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "audit_entity",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "order",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "audit_operation",
                schema: "ordering");
        }
    }
}
