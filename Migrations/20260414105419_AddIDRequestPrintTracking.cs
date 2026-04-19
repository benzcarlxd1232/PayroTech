using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayroTech.Migrations
{
    /// <inheritdoc />
    public partial class AddIDRequestPrintTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "IDRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrinted",
                table: "IDRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrintedAt",
                table: "IDRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "IDRequests");

            migrationBuilder.DropColumn(
                name: "IsPrinted",
                table: "IDRequests");

            migrationBuilder.DropColumn(
                name: "PrintedAt",
                table: "IDRequests");
        }
    }
}
