using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogicalDeviceAndLogicalNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LogicalDeviceId",
                table: "Devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LogicalDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ports = table.Column<string>(type: "jsonb", nullable: false),
                    HierarchyTemplate = table.Column<string>(type: "jsonb", nullable: true),
                    UiTemplate = table.Column<string>(type: "jsonb", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogicalDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogicalNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Parameters = table.Column<string>(type: "jsonb", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogicalNodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogicalDeviceNodes",
                columns: table => new
                {
                    LogicalDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogicalNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeAlias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InstanceCount = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogicalDeviceNodes", x => new { x.LogicalDeviceId, x.LogicalNodeId });
                    table.ForeignKey(
                        name: "FK_LogicalDeviceNodes_LogicalDevices_LogicalDeviceId",
                        column: x => x.LogicalDeviceId,
                        principalTable: "LogicalDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LogicalDeviceNodes_LogicalNodes_LogicalNodeId",
                        column: x => x.LogicalNodeId,
                        principalTable: "LogicalNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_LogicalDeviceId",
                table: "Devices",
                column: "LogicalDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_LogicalDeviceNodes_LogicalNodeId",
                table: "LogicalDeviceNodes",
                column: "LogicalNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_LogicalDevices_Category",
                table: "LogicalDevices",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LogicalDevices_Name",
                table: "LogicalDevices",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogicalNodes_Name",
                table: "LogicalNodes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_LogicalDevices_LogicalDeviceId",
                table: "Devices",
                column: "LogicalDeviceId",
                principalTable: "LogicalDevices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_LogicalDevices_LogicalDeviceId",
                table: "Devices");

            migrationBuilder.DropTable(
                name: "LogicalDeviceNodes");

            migrationBuilder.DropTable(
                name: "LogicalDevices");

            migrationBuilder.DropTable(
                name: "LogicalNodes");

            migrationBuilder.DropIndex(
                name: "IX_Devices_LogicalDeviceId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "LogicalDeviceId",
                table: "Devices");
        }
    }
}
