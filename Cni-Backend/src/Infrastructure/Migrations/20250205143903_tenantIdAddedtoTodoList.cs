using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class tenantIdAddedtoTodoList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "TodoLists",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TodoLists");
        }
    }
}
