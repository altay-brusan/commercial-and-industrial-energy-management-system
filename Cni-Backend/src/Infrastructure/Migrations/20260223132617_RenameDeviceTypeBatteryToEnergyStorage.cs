using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameDeviceTypeBatteryToEnergyStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename root Battery device type
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'EnergyStorage' WHERE "Name" = 'Battery'""");

            // Rename Battery hierarchy sub-types
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'EnergyStorage.Rack' WHERE "Name" = 'Battery.Rack'""");
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'EnergyStorage.Pack' WHERE "Name" = 'Battery.Pack'""");
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'EnergyStorage.Module' WHERE "Name" = 'Battery.Module'""");
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'EnergyStorage.Controller' WHERE "Name" = 'Battery.Controller'""");
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'EnergyStorage.TMS' WHERE "Name" = 'Battery.TMS'""");

            // Update HierarchyTemplate JSON in specification templates
            migrationBuilder.Sql("""UPDATE "DeviceSpecificationTemplates" SET "HierarchyTemplate" = REPLACE("HierarchyTemplate", 'Battery.', 'EnergyStorage.') WHERE "HierarchyTemplate" LIKE '%Battery.%'""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert root EnergyStorage device type
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'Battery' WHERE "Name" = 'EnergyStorage'""");

            // Revert EnergyStorage hierarchy sub-types
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'Battery.Rack' WHERE "Name" = 'EnergyStorage.Rack'""");
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'Battery.Pack' WHERE "Name" = 'EnergyStorage.Pack'""");
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'Battery.Module' WHERE "Name" = 'EnergyStorage.Module'""");
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'Battery.Controller' WHERE "Name" = 'EnergyStorage.Controller'""");
            migrationBuilder.Sql("""UPDATE "DeviceTypes" SET "Name" = 'Battery.TMS' WHERE "Name" = 'EnergyStorage.TMS'""");

            // Revert HierarchyTemplate JSON in specification templates
            migrationBuilder.Sql("""UPDATE "DeviceSpecificationTemplates" SET "HierarchyTemplate" = REPLACE("HierarchyTemplate", 'EnergyStorage.', 'Battery.') WHERE "HierarchyTemplate" LIKE '%EnergyStorage.%'""");
        }
    }
}
