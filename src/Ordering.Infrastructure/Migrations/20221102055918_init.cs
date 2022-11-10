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
                name: "ordering_audit_operation",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ip = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
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
                    table.PrimaryKey("PK_ordering_audit_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordering_order",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    dict = table.Column<string>(type: "JSON", nullable: true),
                    extras = table.Column<string>(type: "JSON", nullable: true),
                    address_street = table.Column<string>(type: "text", nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: true),
                    address_country = table.Column<string>(type: "text", nullable: true),
                    address_zip_code = table.Column<string>(type: "text", nullable: true),
                    rival_networks = table.Column<string>(type: "JSON", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    buyer_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creator_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordering_product",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    price = table.Column<int>(type: "integer", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    creation_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creator_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordering_audit_entity",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    operation_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    operation_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_audit_entity", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_audit_entity_ordering_audit_operation_operation_id",
                        column: x => x.operation_id,
                        principalTable: "ordering_audit_operation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ordering_order_item",
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
                    table.PrimaryKey("PK_ordering_order_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_order_item_ordering_order_order_id",
                        column: x => x.order_id,
                        principalTable: "ordering_order",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ordering_audit_property",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    original_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_audit_property", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_audit_property_ordering_audit_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "ordering_audit_entity",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordering_audit_entity_entity_id",
                table: "ordering_audit_entity",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_audit_entity_operation_id",
                table: "ordering_audit_entity",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_audit_operation_creation_time",
                table: "ordering_audit_operation",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_audit_operation_creator_id",
                table: "ordering_audit_operation",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_audit_operation_end_time",
                table: "ordering_audit_operation",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_audit_property_entity_id",
                table: "ordering_audit_property",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_order_item_order_id",
                table: "ordering_order_item",
                column: "order_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordering_audit_property");

            migrationBuilder.DropTable(
                name: "ordering_order_item");

            migrationBuilder.DropTable(
                name: "ordering_product");

            migrationBuilder.DropTable(
                name: "ordering_audit_entity");

            migrationBuilder.DropTable(
                name: "ordering_order");

            migrationBuilder.DropTable(
                name: "ordering_audit_operation");
        }
    }
}
