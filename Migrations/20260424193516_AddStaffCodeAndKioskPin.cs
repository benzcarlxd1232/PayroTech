using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayroTech.Migrations
{
    /// <inheritdoc />
    public partial class AddStaffCodeAndKioskPin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LateDeductionAmount",
                table: "Attendances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeAmount",
                table: "Attendances",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "KioskPin",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StaffCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LateDeductionAmount",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "OvertimeAmount",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "KioskPin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StaffCode",
                table: "AspNetUsers");
        }
    }
}
