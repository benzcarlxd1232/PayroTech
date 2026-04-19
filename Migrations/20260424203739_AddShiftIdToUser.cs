using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayroTech.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "AspNetUsers");
        }
    }
}
