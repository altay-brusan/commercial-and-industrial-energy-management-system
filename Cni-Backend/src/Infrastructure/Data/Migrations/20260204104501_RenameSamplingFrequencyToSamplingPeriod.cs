using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameSamplingFrequencyToSamplingPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SamplingFrequencyMinutes",
                table: "TenantMarketOperators",
                newName: "SamplingPeriodMinutes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SamplingPeriodMinutes",
                table: "TenantMarketOperators",
                newName: "SamplingFrequencyMinutes");
        }
    }
}
