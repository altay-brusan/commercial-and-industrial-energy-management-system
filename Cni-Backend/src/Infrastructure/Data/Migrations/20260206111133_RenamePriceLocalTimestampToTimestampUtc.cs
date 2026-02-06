using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamePriceLocalTimestampToTimestampUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceLocalTimestamp",
                table: "DayAheadEnergyPriceHistories",
                newName: "TimestampUtc");

            migrationBuilder.RenameIndex(
                name: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country_Price~",
                table: "DayAheadEnergyPriceHistories",
                newName: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country_Times~");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimestampUtc",
                table: "DayAheadEnergyPriceHistories",
                newName: "PriceLocalTimestamp");

            migrationBuilder.RenameIndex(
                name: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country_Times~",
                table: "DayAheadEnergyPriceHistories",
                newName: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country_Price~");
        }
    }
}
