using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ordering.Infrastructure.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditedOperation",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(nullable: false),
                    CreationUserId = table.Column<string>(maxLength: 256, nullable: true),
                    CreationUserName = table.Column<string>(maxLength: 256, nullable: true),
                    ApplicationName = table.Column<string>(maxLength: 256, nullable: true),
                    Path = table.Column<string>(maxLength: 256, nullable: true),
                    Ip = table.Column<string>(maxLength: 256, nullable: true),
                    UserAgent = table.Column<string>(maxLength: 256, nullable: true),
                    EndedTime = table.Column<DateTimeOffset>(nullable: false),
                    Elapsed = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditedOperation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FunctionDefine",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(nullable: false),
                    CreationUserId = table.Column<string>(nullable: true),
                    CreationUserName = table.Column<string>(nullable: true),
                    LastModificationUserId = table.Column<string>(maxLength: 255, nullable: true),
                    LastModificationUserName = table.Column<string>(maxLength: 255, nullable: true),
                    LastModificationTime = table.Column<DateTimeOffset>(nullable: true),
                    Enabled = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Expired = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionDefine", x => x.Id);
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
                name: "AuditedEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TypeName = table.Column<string>(maxLength: 256, nullable: true),
                    EntityId = table.Column<string>(maxLength: 256, nullable: true),
                    OperationType = table.Column<int>(nullable: true),
                    AuditedOperationId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditedEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditedEntity_AuditedOperation_AuditedOperationId",
                        column: x => x.AuditedOperationId,
                        principalTable: "AuditedOperation",
                        principalColumn: "Id",
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
                name: "AuditedProperty",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PropertyName = table.Column<string>(maxLength: 256, nullable: true),
                    PropertyType = table.Column<string>(maxLength: 256, nullable: true),
                    OriginalValue = table.Column<string>(nullable: true),
                    NewValue = table.Column<string>(nullable: true),
                    AuditedEntityId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditedProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditedProperty_AuditedEntity_AuditedEntityId",
                        column: x => x.AuditedEntityId,
                        principalTable: "AuditedEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditedEntity_AuditedOperationId",
                table: "AuditedEntity",
                column: "AuditedOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditedEntity_EntityId",
                table: "AuditedEntity",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditedProperty_AuditedEntityId",
                table: "AuditedProperty",
                column: "AuditedEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionDefine_Code",
                table: "FunctionDefine",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FunctionDefine_Name",
                table: "FunctionDefine",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditedProperty");

            migrationBuilder.DropTable(
                name: "FunctionDefine");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "AuditedEntity");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "AuditedOperation");
        }
    }
}
