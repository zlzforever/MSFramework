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
                    id = table.Column<string>(nullable: false),
                    creation_time = table.Column<int>(type: "int", nullable: true),
                    creation_user_id = table.Column<string>(maxLength: 255, nullable: true),
                    creation_user_name = table.Column<string>(maxLength: 255, nullable: true),
                    application_name = table.Column<string>(maxLength: 255, nullable: true),
                    path = table.Column<string>(maxLength: 255, nullable: true),
                    ip = table.Column<string>(maxLength: 255, nullable: true),
                    user_agent = table.Column<string>(maxLength: 500, nullable: true),
                    end_time = table.Column<DateTimeOffset>(nullable: false),
                    elapsed = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "function",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    creation_time = table.Column<int>(type: "int", nullable: true),
                    creation_user_id = table.Column<string>(maxLength: 255, nullable: true),
                    creation_user_name = table.Column<string>(maxLength: 255, nullable: true),
                    modification_user_id = table.Column<string>(maxLength: 255, nullable: true),
                    modification_user_name = table.Column<string>(maxLength: 255, nullable: true),
                    modification_time = table.Column<int>(type: "int", nullable: true),
                    enabled = table.Column<bool>(nullable: false),
                    name = table.Column<string>(maxLength: 255, nullable: true),
                    code = table.Column<string>(maxLength: 255, nullable: true),
                    description = table.Column<string>(maxLength: 2000, nullable: true),
                    expired = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_function", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    creation_time = table.Column<DateTimeOffset>(nullable: false),
                    address_street = table.Column<string>(nullable: true),
                    address_city = table.Column<string>(nullable: true),
                    address_state = table.Column<string>(nullable: true),
                    address_country = table.Column<string>(nullable: true),
                    address_zip_code = table.Column<string>(nullable: true),
                    order_status = table.Column<int>(nullable: false),
                    user_id = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    is_deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    name = table.Column<string>(maxLength: 256, nullable: true),
                    price = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_entity",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    operation_id = table.Column<string>(nullable: true),
                    type_name = table.Column<string>(maxLength: 255, nullable: true),
                    entity_id = table.Column<string>(maxLength: 255, nullable: true),
                    operation_type = table.Column<string>(maxLength: 255, nullable: true),
                    audit_operation_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entity", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_entity_audit_operation_operation_id",
                        column: x => x.operation_id,
                        principalTable: "audit_operation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_entity_audit_operation_audit_operation_id",
                        column: x => x.audit_operation_id,
                        principalTable: "audit_operation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    product_name = table.Column<string>(nullable: false),
                    picture_url = table.Column<string>(nullable: true),
                    unit_price = table.Column<decimal>(nullable: false),
                    discount = table.Column<decimal>(nullable: false),
                    units = table.Column<int>(nullable: false),
                    product_id = table.Column<Guid>(nullable: false),
                    order_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_item_order_order_id",
                        column: x => x.order_id,
                        principalTable: "order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_property",
                columns: table => new
                {
                    id = table.Column<string>(nullable: false),
                    entity_id = table.Column<string>(nullable: true),
                    name = table.Column<string>(maxLength: 255, nullable: true),
                    type = table.Column<string>(maxLength: 255, nullable: true),
                    original_value = table.Column<string>(nullable: true),
                    new_value = table.Column<string>(nullable: true),
                    audit_entity_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_property", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_property_audit_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "audit_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_property_audit_entity_audit_entity_id",
                        column: x => x.audit_entity_id,
                        principalTable: "audit_entity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_entity_entity_id",
                table: "audit_entity",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entity_operation_id",
                table: "audit_entity",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_entity_audit_operation_id",
                table: "audit_entity",
                column: "audit_operation_id");

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
                name: "IX_audit_property_entity_id",
                table: "audit_property",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_property_audit_entity_id",
                table: "audit_property",
                column: "audit_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_function_code",
                table: "function",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_function_name",
                table: "function",
                column: "name");

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
                name: "function");

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
