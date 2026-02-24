using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkDeviceToSpecificationTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specifications",
                table: "Devices");

            migrationBuilder.AddColumn<string>(
                name: "ParameterMapTemplate",
                table: "DeviceSpecificationTemplates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceSpecificationTemplateId",
                table: "Devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceSpecificationTemplateId",
                table: "Devices",
                column: "DeviceSpecificationTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_DeviceSpecificationTemplates_DeviceSpecificationTem~",
                table: "Devices",
                column: "DeviceSpecificationTemplateId",
                principalTable: "DeviceSpecificationTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_DeviceSpecificationTemplates_DeviceSpecificationTem~",
                table: "Devices");

            migrationBuilder.DropIndex(
                name: "IX_Devices_DeviceSpecificationTemplateId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "ParameterMapTemplate",
                table: "DeviceSpecificationTemplates");

            migrationBuilder.DropColumn(
                name: "DeviceSpecificationTemplateId",
                table: "Devices");

            migrationBuilder.AddColumn<string>(
                name: "Specifications",
                table: "Devices",
                type: "jsonb",
                nullable: true);
        }
    }
}
