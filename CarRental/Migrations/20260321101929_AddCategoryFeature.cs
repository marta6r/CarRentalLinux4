using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category_feature",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    category_id = table.Column<int>(type: "INTEGER", nullable: false),
                    feature_id = table.Column<int>(type: "INTEGER", nullable: false),
                    display_order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_feature", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_feature_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_category_feature_feature_feature_id",
                        column: x => x.feature_id,
                        principalTable: "feature",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_feature_category_id_feature_id",
                table: "category_feature",
                columns: new[] { "category_id", "feature_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_feature_feature_id",
                table: "category_feature",
                column: "feature_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_feature");
        }
    }
}
