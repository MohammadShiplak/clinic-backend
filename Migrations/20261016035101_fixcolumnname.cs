using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class fixcolumnname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CareerStarDate",
                table: "Doctors",
                newName: "CareerStartDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CareerStartDate",
                table: "Doctors",
                newName: "CareerStarDate");
        }
    }
}
