using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameAreaCodeToCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop indexes first (they reference the old column name)
            migrationBuilder.DropIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_AreaCode",
                table: "EnergyPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_AreaCode_PriceLocalTi~",
                table: "EnergyPriceHistories");

            // Rename column (preserves existing data)
            migrationBuilder.RenameColumn(
                name: "AreaCode",
                table: "EnergyPriceHistories",
                newName: "Country");

            // Increase max length from 50 to 100 for country names
            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "EnergyPriceHistories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            // Recreate indexes with new column name
            migrationBuilder.CreateIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_Country",
                table: "EnergyPriceHistories",
                columns: new[] { "MarketOperatorId", "Country" });

            migrationBuilder.CreateIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_Country_PriceLocalTim~",
                table: "EnergyPriceHistories",
                columns: new[] { "MarketOperatorId", "Country", "PriceLocalTimestamp" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_Country",
                table: "EnergyPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_Country_PriceLocalTim~",
                table: "EnergyPriceHistories");

            // Revert max length first
            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "EnergyPriceHistories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            // Rename column back
            migrationBuilder.RenameColumn(
                name: "Country",
                table: "EnergyPriceHistories",
                newName: "AreaCode");

            // Recreate original indexes
            migrationBuilder.CreateIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_AreaCode",
                table: "EnergyPriceHistories",
                columns: new[] { "MarketOperatorId", "AreaCode" });

            migrationBuilder.CreateIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_AreaCode_PriceLocalTi~",
                table: "EnergyPriceHistories",
                columns: new[] { "MarketOperatorId", "AreaCode", "PriceLocalTimestamp" },
                unique: true);
        }
    }
}
