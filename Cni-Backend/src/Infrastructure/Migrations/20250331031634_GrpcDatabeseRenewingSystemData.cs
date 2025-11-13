using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GrpcDatabeseRenewingSystemData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrpcFacilityDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    GeneratorPower = table.Column<double>(type: "float", nullable: false),
                    GridPower = table.Column<double>(type: "float", nullable: false),
                    LoadGeneratorContribution = table.Column<double>(type: "float", nullable: false),
                    LoadGridContribution = table.Column<double>(type: "float", nullable: false),
                    LoadPcsContribution = table.Column<double>(type: "float", nullable: false),
                    LoadSolarContribution = table.Column<double>(type: "float", nullable: false),
                    PcsPower = table.Column<double>(type: "float", nullable: false),
                    SolarPower = table.Column<double>(type: "float", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrpcFacilityDatas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrpcFacilityDatas");
        }
    }
}
