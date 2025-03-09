using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBookstoreAPI0.Migrations
{
    /// <inheritdoc />
    public partial class AddBookCountToGenre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookCount",
                table: "Genres",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookCount",
                table: "Genres");
        }
    }
}
