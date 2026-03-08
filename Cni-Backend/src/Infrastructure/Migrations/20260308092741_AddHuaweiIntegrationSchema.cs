using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHuaweiIntegrationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "huawei");

            migrationBuilder.AddColumn<int>(
                name: "DataSource",
                table: "Facilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "huawei",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EncryptedSystemCode = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RawDataLogs",
                schema: "huawei",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    HuaweiAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RequestPayload = table.Column<string>(type: "jsonb", nullable: true),
                    ResponsePayload = table.Column<string>(type: "jsonb", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: false),
                    CollectedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawDataLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawDataLogs_Accounts_HuaweiAccountId",
                        column: x => x.HuaweiAccountId,
                        principalSchema: "huawei",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationMappings",
                schema: "huawei",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HuaweiAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StationMappings_Accounts_HuaweiAccountId",
                        column: x => x.HuaweiAccountId,
                        principalSchema: "huawei",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StationMappings_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncLogs",
                schema: "huawei",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    HuaweiAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyncType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceCount = table.Column<int>(type: "integer", nullable: false),
                    RecordsInserted = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyncLogs_Accounts_HuaweiAccountId",
                        column: x => x.HuaweiAccountId,
                        principalSchema: "huawei",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceMappings",
                schema: "huawei",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StationMappingId = table.Column<Guid>(type: "uuid", nullable: false),
                    HuaweiDeviceId = table.Column<long>(type: "bigint", nullable: false),
                    HuaweiDevTypeId = table.Column<int>(type: "integer", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceMappings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceMappings_StationMappings_StationMappingId",
                        column: x => x.StationMappingId,
                        principalSchema: "huawei",
                        principalTable: "StationMappings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_TenantId_UserName",
                schema: "huawei",
                table: "Accounts",
                columns: new[] { "TenantId", "UserName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMappings_DeviceId",
                schema: "huawei",
                table: "DeviceMappings",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMappings_StationMappingId_HuaweiDeviceId",
                schema: "huawei",
                table: "DeviceMappings",
                columns: new[] { "StationMappingId", "HuaweiDeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawDataLogs_CollectedAtUtc",
                schema: "huawei",
                table: "RawDataLogs",
                column: "CollectedAtUtc",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_RawDataLogs_HuaweiAccountId",
                schema: "huawei",
                table: "RawDataLogs",
                column: "HuaweiAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StationMappings_FacilityId",
                schema: "huawei",
                table: "StationMappings",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_StationMappings_HuaweiAccountId_StationCode",
                schema: "huawei",
                table: "StationMappings",
                columns: new[] { "HuaweiAccountId", "StationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_HuaweiAccountId_StartedAtUtc",
                schema: "huawei",
                table: "SyncLogs",
                columns: new[] { "HuaweiAccountId", "StartedAtUtc" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceMappings",
                schema: "huawei");

            migrationBuilder.DropTable(
                name: "RawDataLogs",
                schema: "huawei");

            migrationBuilder.DropTable(
                name: "SyncLogs",
                schema: "huawei");

            migrationBuilder.DropTable(
                name: "StationMappings",
                schema: "huawei");

            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "huawei");

            migrationBuilder.DropColumn(
                name: "DataSource",
                table: "Facilities");
        }
    }
}
