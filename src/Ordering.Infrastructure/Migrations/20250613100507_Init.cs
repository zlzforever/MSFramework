using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "socodb_audit_operation",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    ip = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    device_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    device_model = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    lat = table.Column<double>(type: "double precision", nullable: true),
                    lng = table.Column<double>(type: "double precision", nullable: true),
                    user_agent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    end_time = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    elapsed = table.Column<int>(type: "integer", nullable: false),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    creation_time = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    creator_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socodb_audit_operation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "socodb_audit_entity",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    operation_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: true),
                    operation_type = table.Column<string>(type: "varchar", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socodb_audit_entity", x => x.id);
                    table.ForeignKey(
                        name: "FK_socodb_audit_entity_socodb_audit_operation_operation_id",
                        column: x => x.operation_id,
                        principalTable: "socodb_audit_operation",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "socodb_audit_property",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    type = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    original_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socodb_audit_property", x => x.id);
                    table.ForeignKey(
                        name: "FK_socodb_audit_property_socodb_audit_entity_entity_id",
                        column: x => x.entity_id,
                        principalTable: "socodb_audit_entity",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_socodb_audit_entity_entity_id",
                table: "socodb_audit_entity",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_socodb_audit_entity_operation_id",
                table: "socodb_audit_entity",
                column: "operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_socodb_audit_operation_creator_id",
                table: "socodb_audit_operation",
                column: "creator_id");

            migrationBuilder.CreateIndex(
                name: "IX_socodb_audit_operation_end_time",
                table: "socodb_audit_operation",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_socodb_audit_property_entity_id",
                table: "socodb_audit_property",
                column: "entity_id");




        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "socodb_audit_property");

            migrationBuilder.DropTable(
                name: "socodb_order_item");

            migrationBuilder.DropTable(
                name: "socodb_product");

            migrationBuilder.DropTable(
                name: "socodb_audit_entity");

            migrationBuilder.DropTable(
                name: "socodb_order");

            migrationBuilder.DropTable(
                name: "socodb_audit_operation");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
