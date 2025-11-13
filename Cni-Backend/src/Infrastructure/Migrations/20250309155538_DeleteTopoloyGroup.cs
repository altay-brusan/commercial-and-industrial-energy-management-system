using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteTopoloyGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TopologyNodes_TopologyGroups_TopologyGroupId",
                table: "TopologyNodes");

            migrationBuilder.DropTable(
                name: "TopologyGroups");

            migrationBuilder.DropIndex(
                name: "IX_TopologyNodes_TopologyGroupId",
                table: "TopologyNodes");

            migrationBuilder.RenameColumn(
                name: "TopologyGroupId",
                table: "TopologyNodes",
                newName: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "TopologyNodes",
                newName: "TopologyGroupId");

            migrationBuilder.CreateTable(
                name: "TopologyGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Position = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Style = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopologyGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopologyGroups_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopologyGroups_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_TopologyGroupId",
                table: "TopologyNodes",
                column: "TopologyGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyGroups_FacilityId",
                table: "TopologyGroups",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyGroups_TenantId",
                table: "TopologyGroups",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_TopologyNodes_TopologyGroups_TopologyGroupId",
                table: "TopologyNodes",
                column: "TopologyGroupId",
                principalTable: "TopologyGroups",
                principalColumn: "Id");
        }
    }
}
