using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_AppointmentId",
                table: "Notifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReadAtUtc",
                table: "Notifications",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2(2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppointmentId_PersonId_Type",
                table: "Notifications",
                columns: new[] { "AppointmentId", "PersonId", "Type" },
                unique: true,
                filter: "[Type] = 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_AppointmentId_PersonId_Type",
                table: "Notifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReadAtUtc",
                table: "Notifications",
                type: "datetime2(2)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AppointmentId",
                table: "Notifications",
                column: "AppointmentId");
        }
    }
}
