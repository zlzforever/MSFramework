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
                    id = table.Column<Guid>(nullable: false),
                    creation_time = table.Column<DateTimeOffset>(nullable: false),
                    creation_user_id = table.Column<string>(maxLength: 256, nullable: true),
                    creation_user_name = table.Column<string>(maxLength: 256, nullable: true),
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
                    id = table.Column<Guid>(nullable: false),
                    creation_time = table.Column<DateTimeOffset>(nullable: false),
                    creation_user_id = table.Column<string>(maxLength: 256, nullable: true),
                    creation_user_name = table.Column<string>(maxLength: 256, nullable: true),
                    last_modification_user_id = table.Column<string>(maxLength: 256, nullable: true),
                    last_modification_user_name = table.Column<string>(maxLength: 256, nullable: true),
                    last_modification_time = table.Column<DateTimeOffset>(nullable: true),
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
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(nullable: false),
                    Address_Street = table.Column<string>(nullable: true),
                    Address_City = table.Column<string>(nullable: true),
                    Address_State = table.Column<string>(nullable: true),
                    Address_Country = table.Column<string>(nullable: true),
                    Address_ZipCode = table.Column<string>(nullable: true),
                    OrderStatus = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    Price = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_entity",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    type_name = table.Column<string>(maxLength: 255, nullable: true),
                    entity_id = table.Column<string>(maxLength: 255, nullable: true),
                    operation_type = table.Column<string>(maxLength: 255, nullable: true),
                    audit_operation_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_entity", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_entity_audit_operation_audit_operation_id",
                        column: x => x.audit_operation_id,
                        principalTable: "audit_operation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProductName = table.Column<string>(nullable: false),
                    PictureUrl = table.Column<string>(nullable: true),
                    UnitPrice = table.Column<decimal>(nullable: false),
                    Discount = table.Column<decimal>(nullable: false),
                    Units = table.Column<int>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_property",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    name = table.Column<string>(maxLength: 255, nullable: true),
                    type = table.Column<string>(maxLength: 255, nullable: true),
                    original_value = table.Column<string>(nullable: true),
                    new_value = table.Column<string>(nullable: true),
                    audit_entity_id = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_property", x => x.id);
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
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_property");

            migrationBuilder.DropTable(
                name: "function");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "audit_entity");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "audit_operation");
        }
    }
}
