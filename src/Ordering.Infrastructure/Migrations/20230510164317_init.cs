using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "external_user",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordering_audit_operation",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    url = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ip = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    deviceid = table.Column<string>(name: "device_id", type: "character varying(36)", maxLength: 36, nullable: true),
                    devicemodel = table.Column<string>(name: "device_model", type: "character varying(50)", maxLength: 50, nullable: true),
                    lat = table.Column<double>(type: "double precision", nullable: true),
                    lng = table.Column<double>(type: "double precision", nullable: true),
                    useragent = table.Column<string>(name: "user_agent", type: "character varying(1024)", maxLength: 1024, nullable: true),
                    endtime = table.Column<long>(name: "end_time", type: "bigint", nullable: false, defaultValue: 0L),
                    elapsed = table.Column<int>(type: "integer", nullable: false),
                    creationtime = table.Column<long>(name: "creation_time", type: "bigint", nullable: true),
                    creatorid = table.Column<string>(name: "creator_id", type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_audit_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordering_product",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    price = table.Column<int>(type: "integer", nullable: false),
                    concurrencystamp = table.Column<string>(name: "concurrency_stamp", type: "character varying(36)", maxLength: 36, nullable: true),
                    creationtime = table.Column<DateTimeOffset>(name: "creation_time", type: "timestamp with time zone", nullable: true),
                    creatorid = table.Column<string>(name: "creator_id", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ordering_order",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    dict = table.Column<string>(type: "JSON", nullable: true),
                    addressstreet = table.Column<string>(name: "address_street", type: "text", nullable: true),
                    addresscity = table.Column<string>(name: "address_city", type: "text", nullable: true),
                    addressstate = table.Column<string>(name: "address_state", type: "text", nullable: true),
                    addresscountry = table.Column<string>(name: "address_country", type: "text", nullable: true),
                    addresszipcode = table.Column<string>(name: "address_zip_code", type: "text", nullable: true),
                    rivalnetworks = table.Column<string>(name: "rival_networks", type: "JSON", nullable: true),
                    extras = table.Column<string>(type: "JSON", nullable: true),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    buyerid = table.Column<string>(name: "buyer_id", type: "character varying(36)", maxLength: 36, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    creatorid2 = table.Column<string>(name: "creator_id2", type: "character varying(36)", maxLength: 36, nullable: true),
                    concurrencystamp = table.Column<string>(name: "concurrency_stamp", type: "character varying(36)", maxLength: 36, nullable: true),
                    creationtime = table.Column<DateTimeOffset>(name: "creation_time", type: "timestamp with time zone", nullable: true),
                    creatorid = table.Column<string>(name: "creator_id", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_order_external_user_creator_id2",
                        column: x => x.creatorid2,
                        principalTable: "external_user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ordering_audit_entity",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    operationid = table.Column<string>(name: "operation_id", type: "character varying(36)", nullable: true),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    entityid = table.Column<string>(name: "entity_id", type: "character varying(255)", maxLength: 255, nullable: true),
                    operationtype = table.Column<string>(name: "operation_type", type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_audit_entity", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_audit_entity_ordering_audit_operation_operation_id",
                        column: x => x.operationid,
                        principalTable: "ordering_audit_operation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ordering_order_item",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    productname = table.Column<string>(name: "product_name", type: "text", nullable: false),
                    pictureurl = table.Column<string>(name: "picture_url", type: "text", nullable: true),
                    unitprice = table.Column<decimal>(name: "unit_price", type: "numeric", nullable: false),
                    discount = table.Column<decimal>(type: "numeric", nullable: false),
                    units = table.Column<int>(type: "integer", nullable: false),
                    productid = table.Column<Guid>(name: "product_id", type: "uuid", nullable: false),
                    orderid = table.Column<string>(name: "order_id", type: "character varying(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_order_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_order_item_ordering_order_order_id",
                        column: x => x.orderid,
                        principalTable: "ordering_order",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ordering_audit_property",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    entityid = table.Column<string>(name: "entity_id", type: "character varying(36)", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    originalvalue = table.Column<string>(name: "original_value", type: "text", nullable: true),
                    newvalue = table.Column<string>(name: "new_value", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_audit_property", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_audit_property_ordering_audit_entity_entity_id",
                        column: x => x.entityid,
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
                name: "IX_ordering_order_creator_id2",
                table: "ordering_order",
                column: "creator_id2");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_order_item_order_id",
                table: "ordering_order_item",
                column: "order_id");
        }

        /// <inheritdoc />
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

            migrationBuilder.DropTable(
                name: "external_user");
        }
    }
}
