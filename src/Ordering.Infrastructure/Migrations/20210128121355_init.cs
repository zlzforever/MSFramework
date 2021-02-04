using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ordering.Infrastructure.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "audit_operation",
                table => new
                {
                    id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    application_name = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    path = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    url = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    ip = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    user_agent = table.Column<string>("varchar(500) CHARACTER SET utf8mb4", maxLength: 500, nullable: true),
                    end_time = table.Column<DateTimeOffset>("datetime(6)", nullable: false),
                    elapsed = table.Column<int>("int", nullable: false),
                    creation_time = table.Column<long>("int", nullable: true),
                    creation_user_id = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    creation_user_name = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                "function",
                table => new
                {
                    id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    enabled = table.Column<bool>("tinyint(1)", nullable: false),
                    name = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    code = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    description = table.Column<string>("varchar(2000) CHARACTER SET utf8mb4", maxLength: 2000, nullable: true),
                    expired = table.Column<bool>("tinyint(1)", nullable: false),
                    creation_time = table.Column<long>("int", nullable: true),
                    creation_user_id = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    creation_user_name = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    modification_user_id = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    modification_user_name = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    modification_time = table.Column<long>("int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_function", x => x.id);
                });

            migrationBuilder.CreateTable(
                "order",
                table => new
                {
                    id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    creation_time = table.Column<DateTimeOffset>("datetime(6)", nullable: false),
                    address_street = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    address_city = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    address_state = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    address_country = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    address_zip_code = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    order_status = table.Column<int>("int", nullable: false),
                    user_id = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: false),
                    description = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    is_deleted = table.Column<bool>("tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.id);
                });

            migrationBuilder.CreateTable(
                "product",
                table => new
                {
                    id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    name = table.Column<string>("varchar(256) CHARACTER SET utf8mb4", maxLength: 256, nullable: true),
                    price = table.Column<int>("int", nullable: false),
                    concurrency_stamp = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                "audit_entity",
                table => new
                {
                    id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    operation_id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: true),
                    type_name = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    entity_id = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    operation_type = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    audit_operation_id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entity", x => x.id);
                });

            migrationBuilder.CreateTable(
                "order_item",
                table => new
                {
                    id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    product_name = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: false),
                    picture_url = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    unit_price = table.Column<decimal>("decimal(65,30)", nullable: false),
                    discount = table.Column<decimal>("decimal(65,30)", nullable: false),
                    units = table.Column<int>("int", nullable: false),
                    product_id = table.Column<Guid>("char(36)", nullable: false),
                    order_id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                "audit_property",
                table => new
                {
                    id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    entity_id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: true),
                    name = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    type = table.Column<string>("varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    original_value = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    new_value = table.Column<string>("longtext CHARACTER SET utf8mb4", nullable: true),
                    audit_entity_id = table.Column<string>("varchar(36) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_property", x => x.id);
                });

            migrationBuilder.CreateIndex(
                "IX_audit_entity_audit_operation_id",
                "audit_entity",
                "audit_operation_id");

            migrationBuilder.CreateIndex(
                "IX_audit_entity_entity_id",
                "audit_entity",
                "entity_id");

            migrationBuilder.CreateIndex(
                "IX_audit_entity_operation_id",
                "audit_entity",
                "operation_id");

            migrationBuilder.CreateIndex(
                "IX_audit_operation_creation_time",
                "audit_operation",
                "creation_time");

            migrationBuilder.CreateIndex(
                "IX_audit_operation_creation_user_id",
                "audit_operation",
                "creation_user_id");

            migrationBuilder.CreateIndex(
                "IX_audit_operation_end_time",
                "audit_operation",
                "end_time");

            migrationBuilder.CreateIndex(
                "IX_audit_property_audit_entity_id",
                "audit_property",
                "audit_entity_id");

            migrationBuilder.CreateIndex(
                "IX_audit_property_entity_id",
                "audit_property",
                "entity_id");

            migrationBuilder.CreateIndex(
                "IX_function_code",
                "function",
                "code",
                unique: true);

            migrationBuilder.CreateIndex(
                "IX_function_name",
                "function",
                "name");

            migrationBuilder.CreateIndex(
                "IX_order_item_order_id",
                "order_item",
                "order_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "audit_property");

            migrationBuilder.DropTable(
                "function");

            migrationBuilder.DropTable(
                "order_item");

            migrationBuilder.DropTable(
                "product");

            migrationBuilder.DropTable(
                "audit_entity");

            migrationBuilder.DropTable(
                "order");

            migrationBuilder.DropTable(
                "audit_operation");
        }
    }
}
