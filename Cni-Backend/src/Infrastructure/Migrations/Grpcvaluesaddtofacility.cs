using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Grpcvaluesaddtofacility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GrpcChannel",
                table: "Facilities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "GrpcStatus",
                table: "Facilities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GrpcChannel",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "GrpcStatus",
                table: "Facilities");
        }
    }
}
