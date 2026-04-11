using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeriodTracker.Entities.Migrations
{
    /// <inheritdoc />
    public partial class add_pt_cycle_phase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PT_CycleProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TypicalCycleLengthDays = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)28),
                    TypicalPeriodLengthDays = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)5),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_CycleProfiles", x => x.Id);
                    table.CheckConstraint("CK_PT_CycleProfiles_TypicalCycleLengthDays", "[TypicalCycleLengthDays] >= 15 AND [TypicalCycleLengthDays] <= 60");
                    table.CheckConstraint("CK_PT_CycleProfiles_TypicalPeriodLengthDays", "[TypicalPeriodLengthDays] >= 1 AND [TypicalPeriodLengthDays] <= 15");
                    table.ForeignKey(
                        name: "FK_PT_CycleProfiles_FW_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "FW_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PT_Cycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PT_Cycles", x => x.Id);
                    table.CheckConstraint("CK_PT_Cycles_EndDate_After_StartDate", "[EndDate] IS NULL OR [EndDate] >= [StartDate]");
                    table.ForeignKey(
                        name: "FK_PT_Cycles_FW_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "FW_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PT_CycleProfiles_UserId",
                table: "PT_CycleProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PT_Cycles_UserId_EndDate",
                table: "PT_Cycles",
                columns: new[] { "UserId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PT_Cycles_UserId_OpenCycle",
                table: "PT_Cycles",
                column: "UserId",
                unique: true,
                filter: "[EndDate] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PT_Cycles_UserId_StartDate",
                table: "PT_Cycles",
                columns: new[] { "UserId", "StartDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PT_CycleProfiles");

            migrationBuilder.DropTable(
                name: "PT_Cycles");
        }
    }
}
