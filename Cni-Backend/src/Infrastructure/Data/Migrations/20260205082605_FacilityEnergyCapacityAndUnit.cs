using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FacilityEnergyCapacityAndUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CapacityKwh",
                table: "Facilities",
                newName: "EnergyCapacity");

            migrationBuilder.AddColumn<int>(
                name: "EnergyUnit",
                table: "Facilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnergyUnit",
                table: "Facilities");

            migrationBuilder.RenameColumn(
                name: "EnergyCapacity",
                table: "Facilities",
                newName: "CapacityKwh");
        }
    }
}
