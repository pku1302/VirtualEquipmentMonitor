using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualEquipmentMonitor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EquipmentSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    State = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Temperature = table.Column<double>(type: "REAL", nullable: false),
                    Rpm = table.Column<int>(type: "INTEGER", nullable: false),
                    Vibration = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSnapshots_DeviceId_TimestampUtc",
                table: "EquipmentSnapshots",
                columns: new[] { "DeviceId", "TimestampUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EquipmentSnapshots");
        }
    }
}
