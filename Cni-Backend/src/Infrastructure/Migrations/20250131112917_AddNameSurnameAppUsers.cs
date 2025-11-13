using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNameSurnameAppUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AppUsers",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "AppUsers",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "AppUsers");
        }
    }
}
