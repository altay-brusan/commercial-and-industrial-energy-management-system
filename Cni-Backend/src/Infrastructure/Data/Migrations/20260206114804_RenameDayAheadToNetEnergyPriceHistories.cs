using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDayAheadToNetEnergyPriceHistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DayAheadEnergyPriceHistories",
                newName: "DayAheadNetEnergyPriceHistories");

            migrationBuilder.RenameIndex(
                name: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country_Times~",
                table: "DayAheadNetEnergyPriceHistories",
                newName: "IX_DayAheadNetEnergyPriceHistories_MarketOperatorId_Country_Ti~");

            migrationBuilder.RenameIndex(
                name: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country",
                table: "DayAheadNetEnergyPriceHistories",
                newName: "IX_DayAheadNetEnergyPriceHistories_MarketOperatorId_Country");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "DayAheadNetEnergyPriceHistories",
                newName: "DayAheadEnergyPriceHistories");

            migrationBuilder.RenameIndex(
                name: "IX_DayAheadNetEnergyPriceHistories_MarketOperatorId_Country_Ti~",
                table: "DayAheadEnergyPriceHistories",
                newName: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country_Times~");

            migrationBuilder.RenameIndex(
                name: "IX_DayAheadNetEnergyPriceHistories_MarketOperatorId_Country",
                table: "DayAheadEnergyPriceHistories",
                newName: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country");
        }
    }
}
