using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    i_p = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    device_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    device_model = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    lat = table.Column<double>(type: "double precision", nullable: true),
                    lng = table.Column<double>(type: "double precision", nullable: true),
                    user_agent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    end_time = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    elapsed = table.Column<int>(type: "integer", nullable: false),
                    creation_time = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    address_street = table.Column<string>(type: "text", nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: true),
                    address_country = table.Column<string>(type: "text", nullable: true),
                    address_zip_code = table.Column<string>(type: "text", nullable: true),
                    order_status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    price = table.Column<int>(type: "integer", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_entity",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    operation_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    operation_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    audit_operation_id = table.Column<string>(type: "character varying(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entity", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_entity_audit_operation_audit_operation_id",
                        column: x => x.audit_operation_id,
                        principalTable: "audit_operation",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_audit_entity_audit_operation_operation_id",
                        column: x => x.operation_id,
                        principalTable: "audit_operation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    product_name = table.Column<string>(type: "text", nullable: false),
                    picture_url = table.Column<string>(type: "text", nullable: true),
                    unit_price = table.Column<decimal>(type: "numeric", nullable: false),
                    discount = table.Column<decimal>(type: "numeric", nullable: false),
                    units = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<string>(type: "character varying(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_item_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "audit_property",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    original_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    audit_entity_id = table.Column<string>(type: "character varying(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_property", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_property_audit_entity_audit_entity_id",
                        column: x => x.audit_entity_id,
                        principalTable: "audit_entity",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_audit_property_audit_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "audit_entity",
                        principalColumn: "id");
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
                name: "IX_audit_operation_creator_id",
                table: "audit_operation",
                column: "creator_id");

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
                name: "IX_order_item_order_id",
                table: "order_item",
                column: "order_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_property");

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
