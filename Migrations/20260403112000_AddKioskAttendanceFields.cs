using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayroTech.Migrations
{
    /// <inheritdoc />
    public partial class AddKioskAttendanceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QRCodeHash",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FaceEncodingData",
                table: "AspNetUsers",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QRCodeGeneratedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFaceEnrolled",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRCodeHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FaceEncodingData",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "QRCodeGeneratedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsFaceEnrolled",
                table: "AspNetUsers");
        }
    }
}
