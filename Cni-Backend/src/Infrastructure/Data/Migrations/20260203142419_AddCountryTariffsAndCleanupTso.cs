using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryTariffsAndCleanupTso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistributionFee",
                table: "TransmissionSystemOperators");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "TransmissionSystemOperators");

            migrationBuilder.AddColumn<decimal>(
                name: "AppliedTransmissionFee",
                table: "EnergyPriceHistories",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CountryTariffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CountryCode = table.Column<string>(type: "text", nullable: false),
                    CountryName = table.Column<string>(type: "text", nullable: false),
                    DistributionFee = table.Column<decimal>(type: "numeric", nullable: false),
                    TransmissionFee = table.Column<decimal>(type: "numeric", nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryTariffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountryTariffs_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CountryTariffs_TenantId",
                table: "CountryTariffs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountryTariffs");

            migrationBuilder.DropColumn(
                name: "AppliedTransmissionFee",
                table: "EnergyPriceHistories");

            migrationBuilder.AddColumn<decimal>(
                name: "DistributionFee",
                table: "TransmissionSystemOperators",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "TransmissionSystemOperators",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
