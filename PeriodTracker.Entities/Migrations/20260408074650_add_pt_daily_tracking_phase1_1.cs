using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeriodTracker.Entities.Migrations
{
    /// <inheritdoc />
    public partial class add_pt_daily_tracking_phase1_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PT_DailyLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LogDate = table.Column<DateTime>(type: "date", nullable: false),
                    FlowLevel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Mood = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_DailyLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PT_DailyLogs_FW_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "FW_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PT_DailyMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MeasuredDate = table.Column<DateTime>(type: "date", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DecimalValue = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_DailyMetrics", x => x.Id);
                    table.CheckConstraint("CK_PT_DailyMetrics_DecimalValue_Positive", "[DecimalValue] > 0");
                    table.CheckConstraint("CK_PT_DailyMetrics_MetricType_Allowed", "[MetricType] IN ('Weight', 'Temperature', 'Water')");
                    table.ForeignKey(
                        name: "FK_PT_DailyMetrics_FW_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "FW_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PT_Symptoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_Symptoms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PT_DailyLogSymptoms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyLogId = table.Column<int>(type: "int", nullable: false),
                    SymptomId = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<byte>(type: "tinyint", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_DailyLogSymptoms", x => x.Id);
                    table.CheckConstraint("CK_PT_DailyLogSymptoms_Severity_Valid", "[Severity] IS NULL OR ([Severity] >= 1 AND [Severity] <= 5)");
                    table.ForeignKey(
                        name: "FK_PT_DailyLogSymptoms_PT_DailyLogs_DailyLogId",
                        column: x => x.DailyLogId,
                        principalTable: "PT_DailyLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PT_DailyLogSymptoms_PT_Symptoms_SymptomId",
                        column: x => x.SymptomId,
                        principalTable: "PT_Symptoms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PT_DailyLogs_UserId_LogDate",
                table: "PT_DailyLogs",
                columns: new[] { "UserId", "LogDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_DailyLogSymptoms_DailyLogId_SymptomId",
                table: "PT_DailyLogSymptoms",
                columns: new[] { "DailyLogId", "SymptomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_DailyLogSymptoms_SymptomId",
                table: "PT_DailyLogSymptoms",
                column: "SymptomId");

            migrationBuilder.CreateIndex(
                name: "IX_PT_DailyMetrics_UserId_MeasuredDate_MetricType",
                table: "PT_DailyMetrics",
                columns: new[] { "UserId", "MeasuredDate", "MetricType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_DailyMetrics_UserId_MetricType_MeasuredDate",
                table: "PT_DailyMetrics",
                columns: new[] { "UserId", "MetricType", "MeasuredDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_Symptoms_Code",
                table: "PT_Symptoms",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_DailyLogSymptoms");

            migrationBuilder.DropTable(
                name: "PT_DailyMetrics");

            migrationBuilder.DropTable(
                name: "PT_DailyLogs");

            migrationBuilder.DropTable(
                name: "PT_Symptoms");
        }
    }
}
