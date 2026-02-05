using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseSchemaRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys safely using raw SQL (they may not exist in all database states)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Devices"" DROP CONSTRAINT IF EXISTS ""FK_Devices_Devices_SourceDeviceId"";
                ALTER TABLE ""Devices"" DROP CONSTRAINT IF EXISTS ""FK_Devices_Facilities_FacilityId"";
                ALTER TABLE ""Devices"" DROP CONSTRAINT IF EXISTS ""FK_Devices_Tenants_TenantId"";
                ALTER TABLE ""Facilities"" DROP CONSTRAINT IF EXISTS ""FK_Facilities_Tenants_TenantId"";
                ALTER TABLE ""Firms"" DROP CONSTRAINT IF EXISTS ""FK_Firms_Tenants_TenantId"";
            ");

            // Drop tables safely (they may not exist)
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""CountryTariffs"" CASCADE;");

            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS ""TenantMarketOperators"" CASCADE;
                DROP TABLE IF EXISTS ""TodoItems"" CASCADE;
                DROP TABLE IF EXISTS ""TopologyEdges"" CASCADE;
                DROP TABLE IF EXISTS ""TodoLists"" CASCADE;
                DROP TABLE IF EXISTS ""TopologyNodes"" CASCADE;
            ");

            // Drop indexes safely
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ""IX_Facilities_TenantId"";
                DROP INDEX IF EXISTS ""IX_Devices_SourceDeviceId"";
                DROP INDEX IF EXISTS ""IX_Devices_TenantId"";
            ");

            // Drop columns safely (only if they exist)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Tenants"" DROP COLUMN IF EXISTS ""ConnectionString"";
                ALTER TABLE ""Tenants"" DROP COLUMN IF EXISTS ""Items"";
                ALTER TABLE ""Facilities"" DROP COLUMN IF EXISTS ""Capacity"";
                ALTER TABLE ""Facilities"" DROP COLUMN IF EXISTS ""GrpcChannel"";
                ALTER TABLE ""Facilities"" DROP COLUMN IF EXISTS ""GrpcStatus"";
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""DeviceTypes"" DROP COLUMN IF EXISTS ""HasGrpcStream"";
                ALTER TABLE ""DeviceTypes"" DROP COLUMN IF EXISTS ""IsDataCollectedViaOtherDevice"";
                ALTER TABLE ""Devices"" DROP COLUMN IF EXISTS ""GrpcConnection"";
                ALTER TABLE ""Devices"" DROP COLUMN IF EXISTS ""GrpcDeviceStatus"";
                ALTER TABLE ""Devices"" DROP COLUMN IF EXISTS ""GrpcLastDataReviceDate"";
                ALTER TABLE ""Devices"" DROP COLUMN IF EXISTS ""SourceDeviceId"";
                ALTER TABLE ""Devices"" DROP COLUMN IF EXISTS ""TenantId"";
            ");

            // Rename columns safely - check if old column exists and new doesn't
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Facilities' AND column_name='TenantId')
                       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Facilities' AND column_name='CapacityKwh') THEN
                        ALTER TABLE ""Facilities"" RENAME COLUMN ""TenantId"" TO ""CapacityKwh"";
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Devices' AND column_name='MachineId')
                       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Devices' AND column_name='Name') THEN
                        ALTER TABLE ""Devices"" RENAME COLUMN ""MachineId"" TO ""Name"";
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Devices' AND column_name='DeviceName')
                       AND NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Devices' AND column_name='SerialNumber') THEN
                        ALTER TABLE ""Devices"" RENAME COLUMN ""DeviceName"" TO ""SerialNumber"";
                    END IF;
                END $$;
            ");

            // Add columns safely (only if they don't exist)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Facilities' AND column_name='Topology') THEN
                        ALTER TABLE ""Facilities"" ADD COLUMN ""Topology"" text;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Devices' AND column_name='Manufacturer') THEN
                        ALTER TABLE ""Devices"" ADD COLUMN ""Manufacturer"" character varying(200);
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Devices' AND column_name='Model') THEN
                        ALTER TABLE ""Devices"" ADD COLUMN ""Model"" character varying(200) NOT NULL DEFAULT '';
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Devices' AND column_name='Ports') THEN
                        ALTER TABLE ""Devices"" ADD COLUMN ""Ports"" text NOT NULL DEFAULT '';
                    END IF;
                END $$;
            ");

            // Create tables only if they don't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""DeviceLinks"" (
                    ""Id"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""FacilityId"" integer NOT NULL,
                    ""LinkId"" character varying(100) NOT NULL,
                    ""LinkType"" character varying(50) NOT NULL,
                    ""Configuration"" text NOT NULL,
                    ""ConnectedPorts"" text NOT NULL,
                    ""Created"" timestamp with time zone NOT NULL,
                    ""CreatedBy"" text,
                    ""LastModified"" timestamp with time zone NOT NULL,
                    ""LastModifiedBy"" text,
                    CONSTRAINT ""FK_DeviceLinks_Facilities_FacilityId"" FOREIGN KEY (""FacilityId"") REFERENCES ""Facilities"" (""Id"") ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""FirmMarketOperators"" (
                    ""Id"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""FirmId"" integer NOT NULL,
                    ""MarketOperatorId"" integer NOT NULL,
                    ""AreaCode"" character varying(50) NOT NULL,
                    ""SamplingPeriodMinutes"" integer NOT NULL,
                    ""IsActive"" boolean NOT NULL,
                    ""Created"" timestamp with time zone NOT NULL,
                    ""CreatedBy"" text,
                    ""LastModified"" timestamp with time zone NOT NULL,
                    ""LastModifiedBy"" text,
                    CONSTRAINT ""FK_FirmMarketOperators_Firms_FirmId"" FOREIGN KEY (""FirmId"") REFERENCES ""Firms"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_FirmMarketOperators_MarketOperators_MarketOperatorId"" FOREIGN KEY (""MarketOperatorId"") REFERENCES ""MarketOperators"" (""Id"") ON DELETE CASCADE
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Tariffs"" (
                    ""Id"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""CountryName"" character varying(100) NOT NULL,
                    ""DistributorName"" character varying(200),
                    ""DistributionFee"" numeric(18,4) NOT NULL,
                    ""TransmissionFee"" numeric(18,4) NOT NULL,
                    ""TaxRate"" numeric(5,4) NOT NULL,
                    ""IsActive"" boolean NOT NULL DEFAULT true,
                    ""Currency"" character varying(10) NOT NULL,
                    ""Created"" timestamp with time zone NOT NULL,
                    ""CreatedBy"" text,
                    ""LastModified"" timestamp with time zone NOT NULL,
                    ""LastModifiedBy"" text
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""FacilityTariffs"" (
                    ""Id"" integer GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                    ""FacilityId"" integer NOT NULL,
                    ""TariffId"" integer NOT NULL,
                    ""IsActive"" boolean NOT NULL DEFAULT true,
                    ""Created"" timestamp with time zone NOT NULL,
                    ""CreatedBy"" text,
                    ""LastModified"" timestamp with time zone NOT NULL,
                    ""LastModifiedBy"" text,
                    CONSTRAINT ""FK_FacilityTariffs_Facilities_FacilityId"" FOREIGN KEY (""FacilityId"") REFERENCES ""Facilities"" (""Id"") ON DELETE CASCADE,
                    CONSTRAINT ""FK_FacilityTariffs_Tariffs_TariffId"" FOREIGN KEY (""TariffId"") REFERENCES ""Tariffs"" (""Id"") ON DELETE RESTRICT
                );
            ");

            // Create indexes safely (only if they don't exist)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DeviceTypes_Name"" ON ""DeviceTypes"" (""Name"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_DeviceLinks_FacilityId_LinkId"" ON ""DeviceLinks"" (""FacilityId"", ""LinkId"");
                CREATE INDEX IF NOT EXISTS ""IX_FacilityTariffs_FacilityId_IsActive"" ON ""FacilityTariffs"" (""FacilityId"", ""IsActive"") WHERE ""IsActive"" = true;
                CREATE INDEX IF NOT EXISTS ""IX_FacilityTariffs_TariffId"" ON ""FacilityTariffs"" (""TariffId"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_FirmMarketOperators_FirmId_MarketOperatorId"" ON ""FirmMarketOperators"" (""FirmId"", ""MarketOperatorId"");
                CREATE INDEX IF NOT EXISTS ""IX_FirmMarketOperators_MarketOperatorId"" ON ""FirmMarketOperators"" (""MarketOperatorId"");
                CREATE INDEX IF NOT EXISTS ""IX_Tariffs_CountryName"" ON ""Tariffs"" (""CountryName"");
                CREATE INDEX IF NOT EXISTS ""IX_Tariffs_IsActive"" ON ""Tariffs"" (""IsActive"");
            ");

            // Add foreign keys safely (only if they don't exist)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Devices_Facilities_FacilityId') THEN
                        ALTER TABLE ""Devices"" ADD CONSTRAINT ""FK_Devices_Facilities_FacilityId""
                            FOREIGN KEY (""FacilityId"") REFERENCES ""Facilities"" (""Id"") ON DELETE CASCADE;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Firms_Tenants_TenantId') THEN
                        ALTER TABLE ""Firms"" ADD CONSTRAINT ""FK_Firms_Tenants_TenantId""
                            FOREIGN KEY (""TenantId"") REFERENCES ""Tenants"" (""Id"") ON DELETE CASCADE;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Facilities_FacilityId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Firms_Tenants_TenantId",
                table: "Firms");

            migrationBuilder.DropTable(
                name: "DeviceLinks");

            migrationBuilder.DropTable(
                name: "FacilityTariffs");

            migrationBuilder.DropTable(
                name: "FirmMarketOperators");

            migrationBuilder.DropTable(
                name: "Tariffs");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTypes_Name",
                table: "DeviceTypes");

            migrationBuilder.DropColumn(
                name: "Topology",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Ports",
                table: "Devices");

            migrationBuilder.RenameColumn(
                name: "CapacityKwh",
                table: "Facilities",
                newName: "TenantId");

            migrationBuilder.RenameColumn(
                name: "SerialNumber",
                table: "Devices",
                newName: "DeviceName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Devices",
                newName: "MachineId");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionString",
                table: "Tenants",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Items",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Facilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GrpcChannel",
                table: "Facilities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "GrpcStatus",
                table: "Facilities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasGrpcStream",
                table: "DeviceTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDataCollectedViaOtherDevice",
                table: "DeviceTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GrpcConnection",
                table: "Devices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GrpcDeviceStatus",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "GrpcLastDataReviceDate",
                table: "Devices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceDeviceId",
                table: "Devices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Devices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CountryTariffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CountryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "EUR"),
                    DistributionFee = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    TaxRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    TransmissionFee = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
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

            migrationBuilder.CreateTable(
                name: "TenantMarketOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarketOperatorId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    AreaCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    SamplingPeriodMinutes = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "TodoLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Colour_Code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TopologyNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: true),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Extent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Position = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SourceHandleCount = table.Column<int>(type: "integer", nullable: true),
                    Style = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TargetHandleCount = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopologyNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TopologyNodes_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                });

            migrationBuilder.CreateTable(
                name: "TodoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ListId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Done = table.Column<bool>(type: "boolean", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Reminder = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TodoItems_TodoLists_ListId",
                        column: x => x.ListId,
                        principalTable: "TodoLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TopologyEdges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FacilityId = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    SourceHandle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TargetHandle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_Facilities_TenantId",
                table: "Facilities",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SourceDeviceId",
                table: "Devices",
                column: "SourceDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_TenantId",
                table: "Devices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryTariffs_TenantId_CountryCode",
                table: "CountryTariffs",
                columns: new[] { "TenantId", "CountryCode" },
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

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_ListId",
                table: "TodoItems",
                column: "ListId");

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
                name: "IX_TopologyNodes_DeviceId",
                table: "TopologyNodes",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_FacilityId",
                table: "TopologyNodes",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_TopologyNodes_TenantId",
                table: "TopologyNodes",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Devices_SourceDeviceId",
                table: "Devices",
                column: "SourceDeviceId",
                principalTable: "Devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Facilities_FacilityId",
                table: "Devices",
                column: "FacilityId",
                principalTable: "Facilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Tenants_TenantId",
                table: "Devices",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Facilities_Tenants_TenantId",
                table: "Facilities",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Firms_Tenants_TenantId",
                table: "Firms",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
