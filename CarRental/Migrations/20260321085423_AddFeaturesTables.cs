using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Migrations
{
    /// <inheritdoc />
    public partial class AddFeaturesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_car_category_category_id",
                table: "car");

            migrationBuilder.CreateTable(
                name: "feature",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "feature_value",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    value = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    feature_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature_value", x => x.id);
                    table.ForeignKey(
                        name: "FK_feature_value_feature_feature_id",
                        column: x => x.feature_id,
                        principalTable: "feature",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "car_feature",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    car_id = table.Column<int>(type: "INTEGER", nullable: false),
                    feature_id = table.Column<int>(type: "INTEGER", nullable: false),
                    feature_value_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_car_feature", x => x.id);
                    table.ForeignKey(
                        name: "FK_car_feature_car_car_id",
                        column: x => x.car_id,
                        principalTable: "car",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_car_feature_feature_feature_id",
                        column: x => x.feature_id,
                        principalTable: "feature",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_car_feature_feature_value_feature_value_id",
                        column: x => x.feature_value_id,
                        principalTable: "feature_value",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_car_feature_car_id_feature_id",
                table: "car_feature",
                columns: new[] { "car_id", "feature_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_car_feature_feature_id",
                table: "car_feature",
                column: "feature_id");

            migrationBuilder.CreateIndex(
                name: "IX_car_feature_feature_value_id",
                table: "car_feature",
                column: "feature_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_feature_value_feature_id",
                table: "feature_value",
                column: "feature_id");

            migrationBuilder.AddForeignKey(
                name: "FK_car_category_category_id",
                table: "car",
                column: "category_id",
                principalTable: "category",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_car_category_category_id",
                table: "car");

            migrationBuilder.DropTable(
                name: "car_feature");

            migrationBuilder.DropTable(
                name: "feature_value");

            migrationBuilder.DropTable(
                name: "feature");

            migrationBuilder.AddForeignKey(
                name: "FK_car_category_category_id",
                table: "car",
                column: "category_id",
                principalTable: "category",
                principalColumn: "id");
        }
    }
}
