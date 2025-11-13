using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTopologiesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopologyGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Style = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "TopologyNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Extent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TopologyGroupId = table.Column<int>(type: "int", nullable: true),
                    TargetHandleCount = table.Column<int>(type: "int", nullable: true),
                    SourceHandleCount = table.Column<int>(type: "int", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Style = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopologyNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopologyNodes_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopologyNodes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TopologyNodes_TopologyGroups_TopologyGroupId",
                        column: x => x.TopologyGroupId,
                        principalTable: "TopologyGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TopologyEdges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    TargetHandle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceHandle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopologyEdges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopologyEdges_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopologyEdges_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TopologyEdges_TopologyNodes_SourceId",
                        column: x => x.SourceId,
                        principalTable: "TopologyNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopologyEdges_TopologyNodes_TargetId",
                        column: x => x.TargetId,
                        principalTable: "TopologyNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopologyEdges_FacilityId",
                table: "TopologyEdges",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyEdges_SourceId",
                table: "TopologyEdges",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyEdges_TargetId",
                table: "TopologyEdges",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyEdges_TenantId",
                table: "TopologyEdges",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyGroups_FacilityId",
                table: "TopologyGroups",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyGroups_TenantId",
                table: "TopologyGroups",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_FacilityId",
                table: "TopologyNodes",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_TenantId",
                table: "TopologyNodes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_TopologyGroupId",
                table: "TopologyNodes",
                column: "TopologyGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopologyEdges");

            migrationBuilder.DropTable(
                name: "TopologyNodes");

            migrationBuilder.DropTable(
                name: "TopologyGroups");
        }
    }
}
