using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ordering_product",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    price = table.Column<int>(type: "int", nullable: false),
                    concurrency_stamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creation_time = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creator_name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_product", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ordering_order",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_street = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_city = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_state = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_country = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_zip_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    buyer_id2 = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description2 = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    operator_id = table.Column<int>(type: "int", nullable: true),
                    list_json = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    extras = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dict_json = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    concurrency_stamp = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creation_time = table.Column<long>(type: "bigint", nullable: true),
                    creator_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creator_name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_order", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_order_user_operator_id",
                        column: x => x.operator_id,
                        principalTable: "user",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ordering_order_item",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order_id = table.Column<string>(type: "varchar(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    product_id = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    product_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    product_picture_url = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    unit_price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    units = table.Column<int>(type: "int", nullable: false),
                    discount = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordering_order_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_ordering_order_item_ordering_order_order_id",
                        column: x => x.order_id,
                        principalTable: "ordering_order",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_order_creation_time",
                table: "ordering_order",
                column: "creation_time");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_order_operator_id",
                table: "ordering_order",
                column: "operator_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_order_item_order_id",
                table: "ordering_order_item",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_ordering_order_item_product_id",
                table: "ordering_order_item",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordering_order_item");

            migrationBuilder.DropTable(
                name: "ordering_product");

            migrationBuilder.DropTable(
                name: "ordering_order");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
