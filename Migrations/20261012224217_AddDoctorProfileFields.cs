using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CareerStarDate",
                table: "Doctors",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Doctors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_Specialization",
                table: "Doctors",
                column: "Specialization");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Doctors_Specialization",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "CareerStarDate",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Doctors");
        }
    }
}
