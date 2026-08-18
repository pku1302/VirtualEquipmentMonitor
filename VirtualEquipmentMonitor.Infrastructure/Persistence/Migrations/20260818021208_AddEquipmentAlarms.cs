using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualEquipmentMonitor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentAlarms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EquipmentAlarms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MeasuredValue = table.Column<double>(type: "REAL", nullable: false),
                    Threshold = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentAlarms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentAlarms_DeviceId_OccurredAtUtc",
                table: "EquipmentAlarms",
                columns: new[] { "DeviceId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentAlarms");
        }
    }
}
