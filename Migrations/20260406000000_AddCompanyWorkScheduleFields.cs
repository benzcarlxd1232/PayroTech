using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayroTech.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyWorkScheduleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkDaysPerWeek",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "WorkingDaysPerMonth",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 22);

            migrationBuilder.AddColumn<bool>(
                name: "WorkOnHolidays",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HolidayPayRate",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 200);

            migrationBuilder.AddColumn<decimal>(
                name: "HRDailyRate",
                table: "Companies",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AccountantDailyRate",
                table: "Companies",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkDaysPerWeek",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "WorkingDaysPerMonth",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "WorkOnHolidays",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "HolidayPayRate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "HRDailyRate",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "AccountantDailyRate",
                table: "Companies");
        }
    }
}
