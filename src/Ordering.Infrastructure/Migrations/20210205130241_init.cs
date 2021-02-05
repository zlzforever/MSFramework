using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ordering.Infrastructure.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_operation",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    application_name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    feature = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    url = table.Column<string>(type: "varchar(1024) CHARACTER SET utf8mb4", maxLength: 1024, nullable: true),
                    ip = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    user_agent = table.Column<string>(type: "varchar(500) CHARACTER SET utf8mb4", maxLength: 500, nullable: true),
                    end_time = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    elapsed = table.Column<int>(type: "int", nullable: false),
                    creation_time = table.Column<long>(type: "int", nullable: true),
                    creation_user_id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    creation_user_name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feature",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    enabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "varchar(2000) CHARACTER SET utf8mb4", maxLength: 2000, nullable: true),
                    expired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creation_time = table.Column<long>(type: "int", nullable: false),
                    modification_time = table.Column<long>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    creation_time = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    address_street = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    address_city = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    address_state = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    address_country = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    address_zip_code = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    order_status = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: false),
                    user_id = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    description = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    name = table.Column<string>(type: "varchar(256) CHARACTER SET utf8mb4", maxLength: 256, nullable: true),
                    price = table.Column<int>(type: "int", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_entity",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    operation_id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: true),
                    type = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    entity_id = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    operation_type = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    audit_operation_id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entity", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    product_name = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: false),
                    picture_url = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    unit_price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    discount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    units = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<Guid>(type: "char(36)", nullable: false),
                    order_id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_property",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: false),
                    entity_id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: true),
                    name = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "varchar(255) CHARACTER SET utf8mb4", maxLength: 255, nullable: true),
                    original_value = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    new_value = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    audit_entity_id = table.Column<string>(type: "varchar(36) CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_property", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entity_audit_operation_id",
                table: "audit_entity",
                column: "audit_operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entity_entity_id",
                table: "audit_entity",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entity_operation_id",
                table: "audit_entity",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_operation_creation_time",
                table: "audit_operation",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "IX_audit_operation_creation_user_id",
                table: "audit_operation",
                column: "creation_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_operation_end_time",
                table: "audit_operation",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_audit_property_audit_entity_id",
                table: "audit_property",
                column: "audit_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_property_entity_id",
                table: "audit_property",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_feature_name",
                table: "feature",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_item_order_id",
                table: "order_item",
                column: "order_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_property");

            migrationBuilder.DropTable(
                name: "feature");

            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "audit_entity");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "audit_operation");
        }
    }
}
