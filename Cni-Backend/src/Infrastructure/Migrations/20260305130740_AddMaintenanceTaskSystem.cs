using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceTaskSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AcknowledgedAt",
                table: "DeviceAlarms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcknowledgedById",
                table: "DeviceAlarms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaintenanceTaskId",
                table: "DeviceAlarms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResolvedAt",
                table: "DeviceAlarms",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResolvedById",
                table: "DeviceAlarms",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MaintenanceTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedToId = table.Column<string>(type: "text", nullable: false),
                    AssignedById = table.Column<string>(type: "text", nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceAlarmId = table.Column<Guid>(type: "uuid", nullable: true),
                    Resolution = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RootCause = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LaborHours = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    PartsUsed = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Recommendations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedById = table.Column<string>(type: "text", nullable: true),
                    CloseReason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CloseExplanation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedById = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_AppUsers_AssignedById",
                        column: x => x.AssignedById,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_AppUsers_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_AppUsers_ClosedById",
                        column: x => x.ClosedById,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_AppUsers_CompletedById",
                        column: x => x.CompletedById,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_DeviceAlarms_DeviceAlarmId",
                        column: x => x.DeviceAlarmId,
                        principalTable: "DeviceAlarms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceTasks_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskChecklistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedById = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskChecklistItems_AppUsers_CompletedById",
                        column: x => x.CompletedById,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaskChecklistItems_MaintenanceTasks_MaintenanceTaskId",
                        column: x => x.MaintenanceTaskId,
                        principalTable: "MaintenanceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    AuthorId = table.Column<string>(type: "text", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskNotes_AppUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskNotes_MaintenanceTasks_MaintenanceTaskId",
                        column: x => x.MaintenanceTaskId,
                        principalTable: "MaintenanceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskStatusChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ChangedById = table.Column<string>(type: "text", nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStatusChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskStatusChanges_AppUsers_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskStatusChanges_MaintenanceTasks_MaintenanceTaskId",
                        column: x => x.MaintenanceTaskId,
                        principalTable: "MaintenanceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskWorkSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskWorkSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskWorkSessions_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskWorkSessions_MaintenanceTasks_MaintenanceTaskId",
                        column: x => x.MaintenanceTaskId,
                        principalTable: "MaintenanceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_AcknowledgedById",
                table: "DeviceAlarms",
                column: "AcknowledgedById");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAlarms_ResolvedById",
                table: "DeviceAlarms",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_AssignedById",
                table: "MaintenanceTasks",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_AssignedToId_Status",
                table: "MaintenanceTasks",
                columns: new[] { "AssignedToId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_ClosedById",
                table: "MaintenanceTasks",
                column: "ClosedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_CompletedById",
                table: "MaintenanceTasks",
                column: "CompletedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_DeviceAlarmId",
                table: "MaintenanceTasks",
                column: "DeviceAlarmId",
                unique: true,
                filter: "\"DeviceAlarmId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_DeviceId",
                table: "MaintenanceTasks",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_DueDate",
                table: "MaintenanceTasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_FacilityId",
                table: "MaintenanceTasks",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_Status",
                table: "MaintenanceTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistItems_CompletedById",
                table: "TaskChecklistItems",
                column: "CompletedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistItems_MaintenanceTaskId_SortOrder",
                table: "TaskChecklistItems",
                columns: new[] { "MaintenanceTaskId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskNotes_AuthorId",
                table: "TaskNotes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskNotes_MaintenanceTaskId_Created",
                table: "TaskNotes",
                columns: new[] { "MaintenanceTaskId", "Created" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TaskStatusChanges_ChangedById",
                table: "TaskStatusChanges",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_TaskStatusChanges_MaintenanceTaskId_ChangedAt",
                table: "TaskStatusChanges",
                columns: new[] { "MaintenanceTaskId", "ChangedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkSessions_MaintenanceTaskId_StartedAt",
                table: "TaskWorkSessions",
                columns: new[] { "MaintenanceTaskId", "StartedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TaskWorkSessions_UserId",
                table: "TaskWorkSessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAlarms_AppUsers_AcknowledgedById",
                table: "DeviceAlarms",
                column: "AcknowledgedById",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceAlarms_AppUsers_ResolvedById",
                table: "DeviceAlarms",
                column: "ResolvedById",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceAlarms_AppUsers_AcknowledgedById",
                table: "DeviceAlarms");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceAlarms_AppUsers_ResolvedById",
                table: "DeviceAlarms");

            migrationBuilder.DropTable(
                name: "TaskChecklistItems");

            migrationBuilder.DropTable(
                name: "TaskNotes");

            migrationBuilder.DropTable(
                name: "TaskStatusChanges");

            migrationBuilder.DropTable(
                name: "TaskWorkSessions");

            migrationBuilder.DropTable(
                name: "MaintenanceTasks");

            migrationBuilder.DropIndex(
                name: "IX_DeviceAlarms_AcknowledgedById",
                table: "DeviceAlarms");

            migrationBuilder.DropIndex(
                name: "IX_DeviceAlarms_ResolvedById",
                table: "DeviceAlarms");

            migrationBuilder.DropColumn(
                name: "AcknowledgedAt",
                table: "DeviceAlarms");

            migrationBuilder.DropColumn(
                name: "AcknowledgedById",
                table: "DeviceAlarms");

            migrationBuilder.DropColumn(
                name: "MaintenanceTaskId",
                table: "DeviceAlarms");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "DeviceAlarms");

            migrationBuilder.DropColumn(
                name: "ResolvedById",
                table: "DeviceAlarms");
        }
    }
}
