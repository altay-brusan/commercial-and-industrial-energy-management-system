using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFirmFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFirmFacilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    FirmId = table.Column<int>(type: "int", nullable: true),
                    FacilityId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFirmFacilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFirmFacilities_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFirmFacilities_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserFirmFacilities_Firms_FirmId",
                        column: x => x.FirmId,
                        principalTable: "Firms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFirmFacilities_FacilityId",
                table: "UserFirmFacilities",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirmFacilities_FirmId",
                table: "UserFirmFacilities",
                column: "FirmId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFirmFacilities_UserId_FirmId_FacilityId",
                table: "UserFirmFacilities",
                columns: new[] { "UserId", "FirmId", "FacilityId" },
                unique: true,
                filter: "[UserId] IS NOT NULL AND [FirmId] IS NOT NULL AND [FacilityId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFirmFacilities");
        }
    }
}
