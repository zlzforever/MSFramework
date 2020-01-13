using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ordering.Infrastructure.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditOperation",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(maxLength: 255, nullable: true),
                    UserName = table.Column<string>(maxLength: 255, nullable: true),
                    NickName = table.Column<string>(maxLength: 255, nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(nullable: false),
                    FunctionName = table.Column<string>(maxLength: 255, nullable: true),
                    Ip = table.Column<string>(maxLength: 40, nullable: true),
                    UserAgent = table.Column<string>(maxLength: 255, nullable: true),
                    Message = table.Column<string>(maxLength: 500, nullable: true),
                    EndedTime = table.Column<DateTimeOffset>(nullable: false),
                    Elapsed = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditOperation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Function",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(nullable: false),
                    CreationUserId = table.Column<string>(maxLength: 255, nullable: false),
                    CreationUserName = table.Column<string>(maxLength: 255, nullable: false),
                    LastModificationUserId = table.Column<string>(maxLength: 255, nullable: true),
                    LastModificationUserName = table.Column<string>(maxLength: 255, nullable: true),
                    LastModificationTime = table.Column<DateTimeOffset>(nullable: true),
                    Enabled = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    Path = table.Column<string>(maxLength: 255, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    Expired = table.Column<bool>(nullable: false),
                    AuditOperationEnabled = table.Column<bool>(nullable: false),
                    AuditEntityEnabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Function", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Address_Street = table.Column<string>(nullable: true),
                    Address_City = table.Column<string>(nullable: true),
                    Address_State = table.Column<string>(nullable: true),
                    Address_Country = table.Column<string>(nullable: true),
                    Address_ZipCode = table.Column<string>(nullable: true),
                    OrderStatus = table.Column<int>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OperationId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    TypeName = table.Column<string>(maxLength: 255, nullable: true),
                    EntityKey = table.Column<string>(maxLength: 64, nullable: true),
                    OperateType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEntity_AuditOperation_OperationId",
                        column: x => x.OperationId,
                        principalTable: "AuditOperation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Discount = table.Column<decimal>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: true),
                    PictureUrl = table.Column<string>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductName = table.Column<string>(nullable: false),
                    UnitPrice = table.Column<decimal>(nullable: false),
                    Units = table.Column<int>(nullable: false)
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
                name: "AuditProperty",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AuditEntityId = table.Column<Guid>(nullable: false),
                    DisplayName = table.Column<string>(maxLength: 255, nullable: true),
                    FieldName = table.Column<string>(maxLength: 255, nullable: true),
                    OriginalValue = table.Column<string>(nullable: true),
                    NewValue = table.Column<string>(nullable: true),
                    DataType = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditProperty_AuditEntity_AuditEntityId",
                        column: x => x.AuditEntityId,
                        principalTable: "AuditEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntity_OperationId",
                table: "AuditEntity",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditProperty_AuditEntityId",
                table: "AuditProperty",
                column: "AuditEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Function_Name",
                table: "Function",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Function_Path",
                table: "Function",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                table: "OrderItem",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditProperty");

            migrationBuilder.DropTable(
                name: "Function");

            migrationBuilder.DropTable(
                name: "OrderItem");

            migrationBuilder.DropTable(
                name: "AuditEntity");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "AuditOperation");
        }
    }
}
