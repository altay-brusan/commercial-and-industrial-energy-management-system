using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendFacilityTariffsAndRenameToDayAhead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add contract columns to FacilityTariffs
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ContractEndDate",
                table: "FacilityTariffs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                table: "FacilityTariffs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ContractStartDate",
                table: "FacilityTariffs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: "FacilityTariffs",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "FacilityTariffs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            // Rename EnergyPriceHistories table to DayAheadEnergyPriceHistories
            migrationBuilder.RenameTable(
                name: "EnergyPriceHistories",
                newName: "DayAheadEnergyPriceHistories");

            // Rename the primary key constraint
            migrationBuilder.RenameIndex(
                name: "PK_EnergyPriceHistories",
                table: "DayAheadEnergyPriceHistories",
                newName: "PK_DayAheadEnergyPriceHistories");

            // Rename indexes
            migrationBuilder.RenameIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_Country",
                table: "DayAheadEnergyPriceHistories",
                newName: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country");

            migrationBuilder.RenameIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_Country_PriceLocalTim~",
                table: "DayAheadEnergyPriceHistories",
                newName: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country_Price~");

            // Rename foreign key constraint
            migrationBuilder.Sql(
                "ALTER TABLE \"DayAheadEnergyPriceHistories\" " +
                "RENAME CONSTRAINT \"FK_EnergyPriceHistories_MarketOperators_MarketOperatorId\" " +
                "TO \"FK_DayAheadEnergyPriceHistories_MarketOperators_MarketOperator~\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename foreign key constraint back
            migrationBuilder.Sql(
                "ALTER TABLE \"DayAheadEnergyPriceHistories\" " +
                "RENAME CONSTRAINT \"FK_DayAheadEnergyPriceHistories_MarketOperators_MarketOperator~\" " +
                "TO \"FK_EnergyPriceHistories_MarketOperators_MarketOperatorId\"");

            // Rename indexes back
            migrationBuilder.RenameIndex(
                name: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country_Price~",
                table: "DayAheadEnergyPriceHistories",
                newName: "IX_EnergyPriceHistories_MarketOperatorId_Country_PriceLocalTim~");

            migrationBuilder.RenameIndex(
                name: "IX_DayAheadEnergyPriceHistories_MarketOperatorId_Country",
                table: "DayAheadEnergyPriceHistories",
                newName: "IX_EnergyPriceHistories_MarketOperatorId_Country");

            // Rename primary key back
            migrationBuilder.RenameIndex(
                name: "PK_DayAheadEnergyPriceHistories",
                table: "DayAheadEnergyPriceHistories",
                newName: "PK_EnergyPriceHistories");

            // Rename table back
            migrationBuilder.RenameTable(
                name: "DayAheadEnergyPriceHistories",
                newName: "EnergyPriceHistories");

            // Remove contract columns from FacilityTariffs
            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "FacilityTariffs");

            migrationBuilder.DropColumn(
                name: "ContractNumber",
                table: "FacilityTariffs");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "FacilityTariffs");

            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "FacilityTariffs");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "FacilityTariffs");
        }
    }
}
