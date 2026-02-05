using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameToMarketOperatorsAndRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnergyPriceHistories_Tenants_TenantId",
                table: "EnergyPriceHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_EnergyPriceHistories_TransmissionSystemOperators_TsoId",
                table: "EnergyPriceHistories");

            migrationBuilder.DropTable(
                name: "TransmissionSystemOperators");

            migrationBuilder.DropIndex(
                name: "IX_EnergyPriceHistories_TenantId",
                table: "EnergyPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_EnergyPriceHistories_TsoId",
                table: "EnergyPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_CountryTariffs_TenantId",
                table: "CountryTariffs");

            migrationBuilder.DropColumn(
                name: "AppliedDistributionFee",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "AppliedTaxRate",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "AppliedTransmissionFee",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "RawPrice",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "EnergyPriceHistories");

            migrationBuilder.RenameColumn(
                name: "TsoId",
                table: "EnergyPriceHistories",
                newName: "MarketOperatorId");

            migrationBuilder.RenameColumn(
                name: "PriceTimestamp",
                table: "EnergyPriceHistories",
                newName: "PriceLocalTimestamp");

            migrationBuilder.AddColumn<string>(
                name: "AreaCode",
                table: "EnergyPriceHistories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GrossPrice",
                table: "EnergyPriceHistories",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetPrice",
                table: "EnergyPriceHistories",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TransmissionFee",
                table: "CountryTariffs",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxRate",
                table: "CountryTariffs",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "DistributionFee",
                table: "CountryTariffs",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<string>(
                name: "CountryName",
                table: "CountryTariffs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "CountryTariffs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "CountryTariffs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "EUR");

            migrationBuilder.CreateTable(
                name: "MarketOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SupportedResolutions = table.Column<List<int>>(type: "jsonb", nullable: false),
                    AreaCodes = table.Column<List<string>>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketOperators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantMarketOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    MarketOperatorId = table.Column<int>(type: "integer", nullable: false),
                    AreaCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SamplingFrequencyMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantMarketOperators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantMarketOperators_MarketOperators_MarketOperatorId",
                        column: x => x.MarketOperatorId,
                        principalTable: "MarketOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantMarketOperators_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_AreaCode",
                table: "EnergyPriceHistories",
                columns: new[] { "MarketOperatorId", "AreaCode" });

            migrationBuilder.CreateIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_AreaCode_PriceLocalTi~",
                table: "EnergyPriceHistories",
                columns: new[] { "MarketOperatorId", "AreaCode", "PriceLocalTimestamp" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CountryTariffs_TenantId_CountryCode",
                table: "CountryTariffs",
                columns: new[] { "TenantId", "CountryCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketOperators_Code",
                table: "MarketOperators",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantMarketOperators_MarketOperatorId",
                table: "TenantMarketOperators",
                column: "MarketOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantMarketOperators_TenantId_MarketOperatorId",
                table: "TenantMarketOperators",
                columns: new[] { "TenantId", "MarketOperatorId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EnergyPriceHistories_MarketOperators_MarketOperatorId",
                table: "EnergyPriceHistories",
                column: "MarketOperatorId",
                principalTable: "MarketOperators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnergyPriceHistories_MarketOperators_MarketOperatorId",
                table: "EnergyPriceHistories");

            migrationBuilder.DropTable(
                name: "TenantMarketOperators");

            migrationBuilder.DropTable(
                name: "MarketOperators");

            migrationBuilder.DropIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_AreaCode",
                table: "EnergyPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_EnergyPriceHistories_MarketOperatorId_AreaCode_PriceLocalTi~",
                table: "EnergyPriceHistories");

            migrationBuilder.DropIndex(
                name: "IX_CountryTariffs_TenantId_CountryCode",
                table: "CountryTariffs");

            migrationBuilder.DropColumn(
                name: "AreaCode",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "GrossPrice",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "NetPrice",
                table: "EnergyPriceHistories");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "CountryTariffs");

            migrationBuilder.RenameColumn(
                name: "PriceLocalTimestamp",
                table: "EnergyPriceHistories",
                newName: "PriceTimestamp");

            migrationBuilder.RenameColumn(
                name: "MarketOperatorId",
                table: "EnergyPriceHistories",
                newName: "TsoId");

            migrationBuilder.AddColumn<decimal>(
                name: "AppliedDistributionFee",
                table: "EnergyPriceHistories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AppliedTaxRate",
                table: "EnergyPriceHistories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AppliedTransmissionFee",
                table: "EnergyPriceHistories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "EnergyPriceHistories",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RawPrice",
                table: "EnergyPriceHistories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "EnergyPriceHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "EnergyPriceHistories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "TransmissionFee",
                table: "CountryTariffs",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxRate",
                table: "CountryTariffs",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "DistributionFee",
                table: "CountryTariffs",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,6)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<string>(
                name: "CountryName",
                table: "CountryTariffs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CountryCode",
                table: "CountryTariffs",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "TransmissionSystemOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<string>(type: "text", nullable: true),
                    AreaCode = table.Column<string>(type: "text", nullable: true),
                    BaseUrl = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TimeResolutionMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmissionSystemOperators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransmissionSystemOperators_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnergyPriceHistories_TenantId",
                table: "EnergyPriceHistories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyPriceHistories_TsoId",
                table: "EnergyPriceHistories",
                column: "TsoId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryTariffs_TenantId",
                table: "CountryTariffs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TransmissionSystemOperators_TenantId",
                table: "TransmissionSystemOperators",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnergyPriceHistories_Tenants_TenantId",
                table: "EnergyPriceHistories",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EnergyPriceHistories_TransmissionSystemOperators_TsoId",
                table: "EnergyPriceHistories",
                column: "TsoId",
                principalTable: "TransmissionSystemOperators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
