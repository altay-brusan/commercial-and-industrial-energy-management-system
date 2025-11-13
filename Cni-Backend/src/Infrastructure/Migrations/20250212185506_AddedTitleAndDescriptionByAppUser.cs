using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedTitleAndDescriptionByAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppUsers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "AppUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "AppUsers");
        }
    }
}
