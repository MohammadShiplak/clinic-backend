using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailAttempts",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmailLastError",
                table: "Notifications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailSentAtUtc",
                table: "Notifications",
                type: "datetime2(2)",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "EmailStatus",
                table: "Notifications",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_EmailStatus_NotificationId",
                table: "Notifications",
                columns: new[] { "EmailStatus", "NotificationId" },
                filter: "[EmailStatus] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_EmailStatus_NotificationId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailAttempts",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailLastError",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailSentAtUtc",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "EmailStatus",
                table: "Notifications");
        }
    }
}
