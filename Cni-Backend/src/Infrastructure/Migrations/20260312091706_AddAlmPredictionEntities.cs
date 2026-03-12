using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlmPredictionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "alm");

            migrationBuilder.AddColumn<bool>(
                name: "AlmEnabled",
                table: "Facilities",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PredictionJobs",
                schema: "alm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExternalJobId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ResultStatusCode = table.Column<int>(type: "integer", nullable: true),
                    ResultMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ErrorDetails = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DataPointCount = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictionJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredictionJobs_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForecastPoints",
                schema: "alm",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    AlmPredictionJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Step = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastPoints_PredictionJobs_AlmPredictionJobId",
                        column: x => x.AlmPredictionJobId,
                        principalSchema: "alm",
                        principalTable: "PredictionJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastPoints_AlmPredictionJobId_Timestamp",
                schema: "alm",
                table: "ForecastPoints",
                columns: new[] { "AlmPredictionJobId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PredictionJobs_ExternalJobId",
                schema: "alm",
                table: "PredictionJobs",
                column: "ExternalJobId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictionJobs_FacilityId_JobType_Status",
                schema: "alm",
                table: "PredictionJobs",
                columns: new[] { "FacilityId", "JobType", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ForecastPoints",
                schema: "alm");

            migrationBuilder.DropTable(
                name: "PredictionJobs",
                schema: "alm");

            migrationBuilder.DropColumn(
                name: "AlmEnabled",
                table: "Facilities");
        }
    }
}
