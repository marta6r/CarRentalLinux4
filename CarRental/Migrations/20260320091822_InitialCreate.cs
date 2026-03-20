using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "car",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    brand = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "INTEGER", nullable: false),
                    daily_price = table.Column<decimal>(type: "decimal", nullable: false),
                    is_available = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_car", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    full_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    passport_number = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rental",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    car_id = table.Column<int>(type: "INTEGER", nullable: false),
                    customer_id = table.Column<int>(type: "INTEGER", nullable: false),
                    rent_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    return_date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    total_price = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rental", x => x.id);
                    table.ForeignKey(
                        name: "FK_rental_car_car_id",
                        column: x => x.car_id,
                        principalTable: "car",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rental_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rental_car_id",
                table: "rental",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_rental_customer_id",
                table: "rental",
                column: "customer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rental");

            migrationBuilder.DropTable(
                name: "car");

            migrationBuilder.DropTable(
                name: "customer");
        }
    }
}
