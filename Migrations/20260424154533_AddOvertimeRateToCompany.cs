using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayroTech.Migrations
{
    /// <inheritdoc />
    public partial class AddOvertimeRateToCompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeRatePerHour",
                table: "Companies",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OvertimeRatePerHour",
                table: "Companies");
        }
    }
}
